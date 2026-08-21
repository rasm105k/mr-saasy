using MR.SAASy.Contracts.Motor.Domain;
using MR.SAASy.Contracts.Motor.Events;
using MR.SAASy.Contracts.Motor.Mcp;

namespace MR.SAASy.Core.Motor.Mcp;

/// <summary>
/// The only MOTOR path to external tools. Permission evaluation occurs before connector
/// resolution and invocation, and all attempts emit sanitized audit events.
/// </summary>
public sealed class McpGateway : IMcpGateway
{
    private readonly IMcpPermissionEvaluator _permissionEvaluator;
    private readonly IMotorEventSink _eventSink;
    private readonly TimeProvider _timeProvider;
    private readonly IReadOnlyDictionary<McpConnectorKey, IMcpConnector> _connectors;

    public McpGateway(
        IMcpPermissionEvaluator permissionEvaluator,
        IMotorEventSink eventSink,
        TimeProvider timeProvider,
        IEnumerable<IMcpConnector> connectors)
    {
        _permissionEvaluator = permissionEvaluator ?? throw new ArgumentNullException(nameof(permissionEvaluator));
        _eventSink = eventSink ?? throw new ArgumentNullException(nameof(eventSink));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        ArgumentNullException.ThrowIfNull(connectors);

        var byKey = new Dictionary<McpConnectorKey, IMcpConnector>();
        foreach (var connector in connectors)
        {
            if (!byKey.TryAdd(connector.Key, connector))
            {
                throw new ArgumentException($"Duplicate MCP connector '{connector.Key}'.", nameof(connectors));
            }
        }

        _connectors = byKey;
    }

    public async ValueTask<McpCallResult> ExecuteAsync(
        McpGatewayRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var startedAt = _timeProvider.GetUtcNow();
        var permission = await _permissionEvaluator.EvaluateAsync(request, cancellationToken);

        if (!permission.IsAllowed)
        {
            var denied = new McpCallResult(
                request.ToolCall.Id,
                ExecutionOutcome.Denied,
                permission.Reason,
                _timeProvider.GetUtcNow() - startedAt);
            await RecordAttemptAsync(request, denied, cancellationToken);
            return denied;
        }

        if (!_connectors.TryGetValue(request.ToolCall.Connector, out var connector))
        {
            var unavailable = new McpCallResult(
                request.ToolCall.Id,
                ExecutionOutcome.Failed,
                "No MCP connector is registered for the requested key.",
                _timeProvider.GetUtcNow() - startedAt);
            await RecordAttemptAsync(request, unavailable, cancellationToken);
            return unavailable;
        }

        McpCallResult result;
        try
        {
            var connectorResult = await connector.ExecuteAsync(request, cancellationToken);
            result = connectorResult with
            {
                ToolCallId = request.ToolCall.Id,
                Duration = _timeProvider.GetUtcNow() - startedAt,
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            result = new McpCallResult(
                request.ToolCall.Id,
                ExecutionOutcome.Failed,
                "The MCP connector failed. Provider details were not written to the event stream.",
                _timeProvider.GetUtcNow() - startedAt);
        }

        await RecordAttemptAsync(request, result, cancellationToken);
        return result;
    }

    private ValueTask RecordAttemptAsync(
        McpGatewayRequest request,
        McpCallResult result,
        CancellationToken cancellationToken) =>
        _eventSink.RecordAsync(
            new ToolCalled(
                new MotorEventId(Guid.NewGuid().ToString("N")),
                request.ToolCall.MissionId,
                request.Project,
                _timeProvider.GetUtcNow(),
                request.ToolCall.CorrelationId,
                request.ToolCall.Id,
                request.ToolCall.AgentId,
                request.ToolCall.Connector,
                request.ToolCall.Operation,
                result.Outcome,
                result.ResultReference),
            cancellationToken);
}
