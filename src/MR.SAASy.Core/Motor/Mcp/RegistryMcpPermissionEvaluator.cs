using System.Text.RegularExpressions;
using MR.SAASy.Contracts.Motor.Agents;
using MR.SAASy.Contracts.Motor.Domain;
using MR.SAASy.Contracts.Motor.Mcp;

namespace MR.SAASy.Core.Motor.Mcp;

/// <summary>Fail-closed evaluator backed by the explicit MOTOR agent registry.</summary>
public sealed class RegistryMcpPermissionEvaluator : IMcpPermissionEvaluator
{
    private readonly IAgentRegistry _agentRegistry;
    private readonly TimeProvider _timeProvider;

    public RegistryMcpPermissionEvaluator(IAgentRegistry agentRegistry, TimeProvider timeProvider)
    {
        _agentRegistry = agentRegistry ?? throw new ArgumentNullException(nameof(agentRegistry));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async ValueTask<McpPermissionDecision> EvaluateAsync(
        McpGatewayRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var call = request.ToolCall;
        if (call.RequiredPermission == PermissionLevel.Denied)
        {
            return Deny("A tool call must name a non-zero required permission.");
        }

        var agent = await _agentRegistry.FindAsync(call.AgentId, cancellationToken);
        if (agent is null)
        {
            return Deny("Agent is not registered in MOTOR.");
        }

        var permission = agent.Permissions.FirstOrDefault(candidate =>
            string.Equals(candidate.Connector, call.Connector.Value, StringComparison.Ordinal) &&
            string.Equals(candidate.Operation, call.Operation, StringComparison.Ordinal));

        if (permission is null || permission.Level < call.RequiredPermission)
        {
            return Deny("No explicit agent permission covers the connector operation and level.");
        }

        var actionReference = $"{call.Connector.Value}.{call.Operation}";
        var declaredRequirement = agent.ApprovalRequirements.Any(requirement =>
            call.Risk >= requirement.MinimumRisk && PatternMatches(requirement.ActionPattern, actionReference));
        var requiresApproval =
            call.IsDestructive ||
            call.RequiredPermission == PermissionLevel.Delete ||
            (call.Risk == RiskLevel.Unknown && call.RequiredPermission >= PermissionLevel.Write) ||
            permission.RequiresApproval ||
            declaredRequirement;

        if (!requiresApproval)
        {
            return new McpPermissionDecision(true, false, "Explicit agent permission granted.");
        }

        if (request.Approval is not { } approval ||
            approval.MissionId != call.MissionId ||
            !SameProject(approval.Project, request.Project) ||
            !string.Equals(approval.ActionReference, actionReference, StringComparison.Ordinal) ||
            !approval.IsApproved(_timeProvider.GetUtcNow()))
        {
            return new McpPermissionDecision(false, true,
                "The action requires a current approval bound to this mission, project and operation.");
        }

        return new McpPermissionDecision(true, true, "Explicit permission and bound human approval granted.");
    }

    private static McpPermissionDecision Deny(string reason) => new(false, false, reason);

    private static bool SameProject(MotorProjectContext left, MotorProjectContext right) =>
        string.Equals(left.WorkspaceId, right.WorkspaceId, StringComparison.Ordinal) &&
        string.Equals(left.ProjectId, right.ProjectId, StringComparison.Ordinal) &&
        string.Equals(left.Environment, right.Environment, StringComparison.Ordinal) &&
        string.Equals(left.CustomerId, right.CustomerId, StringComparison.Ordinal);

    private static bool PatternMatches(string pattern, string value)
    {
        var expression = $"^{Regex.Escape(pattern).Replace("\\*", ".*", StringComparison.Ordinal)}$";
        return Regex.IsMatch(value, expression, RegexOptions.CultureInvariant);
    }
}
