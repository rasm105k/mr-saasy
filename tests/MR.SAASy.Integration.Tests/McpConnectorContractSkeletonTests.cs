using MR.SAASy.Contracts.Motor.Agents;
using MR.SAASy.Contracts.Motor.Domain;
using MR.SAASy.Contracts.Motor.Events;
using MR.SAASy.Contracts.Motor.Mcp;
using MR.SAASy.Core.Motor.Agents;
using MR.SAASy.Core.Motor.Events;
using MR.SAASy.Core.Motor.Mcp;
using Xunit;

namespace MR.SAASy.Integration.Tests;

/// <summary>
/// Executable adapter contract skeleton. Live connector suites can replace ContractConnector
/// without changing the gateway or permission-boundary assertions.
/// </summary>
public sealed class McpConnectorContractSkeletonTests
{
    [Theory]
    [InlineData("azure")]
    [InlineData("github")]
    [InlineData("linear")]
    [InlineData("workslip")]
    public async Task Connector_is_reachable_only_through_the_permission_checked_gateway(string connectorName)
    {
        var at = new DateTimeOffset(2026, 8, 21, 20, 0, 0, TimeSpan.Zero);
        var connectorKey = new McpConnectorKey(connectorName);
        var agent = new AgentDefinition(
            new AgentId("contract-agent"),
            "Contract Agent",
            "Integration contract probe",
            AgentAccessMode.ReadOnly,
            ["contract-test"],
            [new AgentPermission(connectorName, "health.read", PermissionLevel.Read)],
            [new AgentApprovalRequirement("*.write", RiskLevel.Low, "No mutation in contract tests")]);
        var registry = new InMemoryAgentRegistry([agent]);
        var clock = new FixedTimeProvider(at);
        var events = new InMemoryMotorEventSink();
        var connector = new ContractConnector(connectorKey);
        var gateway = new McpGateway(
            new RegistryMcpPermissionEvaluator(registry, clock),
            events,
            clock,
            [connector]);
        var request = new McpGatewayRequest(
            new ToolCall(
                new ToolCallId($"{connectorName}-call"),
                new MissionId("mission-contract"),
                agent.Id,
                connectorKey,
                "health.read",
                PermissionLevel.Read,
                RiskLevel.Low,
                false,
                ToolCallState.Requested,
                at,
                "corr-contract"),
            new MotorProjectContext("workspace", "motor", "MOTOR", "test"),
            new Dictionary<string, string?>());

        var result = await gateway.ExecuteAsync(request);

        Assert.Equal(ExecutionOutcome.Succeeded, result.Outcome);
        Assert.Equal(1, connector.Invocations);
        var recorded = Assert.IsType<ToolCalled>(Assert.Single(events.Events));
        Assert.Equal(connectorKey, recorded.Connector);
    }

    private sealed class ContractConnector(McpConnectorKey key) : IMcpConnector
    {
        public McpConnectorKey Key { get; } = key;

        public int Invocations { get; private set; }

        public ValueTask<McpCallResult> ExecuteAsync(
            McpGatewayRequest request,
            CancellationToken cancellationToken = default)
        {
            Invocations++;
            return ValueTask.FromResult(new McpCallResult(
                request.ToolCall.Id,
                ExecutionOutcome.Succeeded,
                "Contract connector completed.",
                TimeSpan.Zero,
                $"{Key.Value}://health"));
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
