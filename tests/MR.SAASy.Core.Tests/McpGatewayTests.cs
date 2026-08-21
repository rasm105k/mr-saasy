using MR.SAASy.Contracts.Motor.Agents;
using MR.SAASy.Contracts.Motor.Domain;
using MR.SAASy.Contracts.Motor.Events;
using MR.SAASy.Contracts.Motor.Mcp;
using MR.SAASy.Core.Motor.Agents;
using MR.SAASy.Core.Motor.Events;
using MR.SAASy.Core.Motor.Mcp;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class McpGatewayTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 21, 20, 0, 0, TimeSpan.Zero);
    private static readonly MotorProjectContext Project = new("workspace", "project", "Project", "test");

    [Fact]
    public async Task Permission_is_checked_before_external_connector_invocation()
    {
        var connector = new StubConnector(KnownMcpConnectors.Azure);
        var events = new InMemoryMotorEventSink();
        var gateway = Gateway(events, connector);
        var request = Request(
            new AgentId("gordon"),
            KnownMcpConnectors.Azure,
            "permissions.write",
            PermissionLevel.Write,
            arguments: new Dictionary<string, string?> { ["secret"] = "must-not-be-logged" });

        var result = await gateway.ExecuteAsync(request);

        Assert.Equal(ExecutionOutcome.Denied, result.Outcome);
        Assert.Equal(0, connector.Invocations);
        var toolEvent = Assert.IsType<ToolCalled>(Assert.Single(events.Events));
        Assert.Equal("permissions.write", toolEvent.Operation);
        Assert.DoesNotContain("must-not-be-logged", new SystemTextJsonMotorEventSerializer().Serialize(toolEvent), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Approved_exact_operation_reaches_the_registered_connector()
    {
        var connector = new StubConnector(KnownMcpConnectors.GitHub);
        var events = new InMemoryMotorEventSink();
        var gateway = Gateway(events, connector);
        var approval = Approved("github.pull-request.merge");
        var request = Request(
            new AgentId("forge"),
            KnownMcpConnectors.GitHub,
            "pull-request.merge",
            PermissionLevel.Write,
            approval: approval);

        var result = await gateway.ExecuteAsync(request);

        Assert.Equal(ExecutionOutcome.Succeeded, result.Outcome);
        Assert.Equal(1, connector.Invocations);
        Assert.Equal(ExecutionOutcome.Succeeded, Assert.IsType<ToolCalled>(Assert.Single(events.Events)).Outcome);
    }

    [Fact]
    public async Task Destructive_action_is_denied_without_bound_approval()
    {
        var connector = new StubConnector(KnownMcpConnectors.Azure);
        var events = new InMemoryMotorEventSink();
        var gateway = Gateway(events, connector);
        var request = Request(
            new AgentId("cleanup-guardian"),
            KnownMcpConnectors.Azure,
            "resource.delete",
            PermissionLevel.Delete,
            isDestructive: true);

        var result = await gateway.ExecuteAsync(request);

        Assert.Equal(ExecutionOutcome.Denied, result.Outcome);
        Assert.Contains("approval", result.Reason, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, connector.Invocations);
    }

    [Fact]
    public async Task Connector_failure_is_sanitized_and_audited()
    {
        var connector = new StubConnector(KnownMcpConnectors.GitHub, shouldThrow: true);
        var events = new InMemoryMotorEventSink();
        var gateway = Gateway(events, connector);
        var request = Request(
            new AgentId("forge"),
            KnownMcpConnectors.GitHub,
            "repository.read",
            PermissionLevel.Read);

        var result = await gateway.ExecuteAsync(request);

        Assert.Equal(ExecutionOutcome.Failed, result.Outcome);
        Assert.DoesNotContain("provider-secret-error", result.Reason, StringComparison.Ordinal);
        Assert.Equal(ExecutionOutcome.Failed, Assert.IsType<ToolCalled>(Assert.Single(events.Events)).Outcome);
    }

    [Fact]
    public async Task Delete_level_is_always_treated_as_destructive_even_if_caller_misclassifies_it()
    {
        var agent = new AgentDefinition(
            new AgentId("unsafe-delete-agent"),
            "Unsafe Delete Agent",
            "Test",
            AgentAccessMode.RestrictedWrite,
            ["cleanup"],
            [new AgentPermission("azure", "resource.delete", PermissionLevel.Delete)],
            [new AgentApprovalRequirement("unrelated.*", RiskLevel.Low, "Unrelated")]);
        var registry = new InMemoryAgentRegistry([agent]);
        var clock = new FixedTimeProvider(Now);
        var connector = new StubConnector(KnownMcpConnectors.Azure);
        var gateway = new McpGateway(
            new RegistryMcpPermissionEvaluator(registry, clock),
            new InMemoryMotorEventSink(),
            clock,
            [connector]);
        var request = Request(
            agent.Id,
            KnownMcpConnectors.Azure,
            "resource.delete",
            PermissionLevel.Delete,
            isDestructive: false);

        var result = await gateway.ExecuteAsync(request);

        Assert.Equal(ExecutionOutcome.Denied, result.Outcome);
        Assert.Equal(0, connector.Invocations);
    }

    private static McpGateway Gateway(InMemoryMotorEventSink events, params IMcpConnector[] connectors)
    {
        var registry = new InMemoryAgentRegistry(DefaultMotorAgentCatalog.Create());
        var clock = new FixedTimeProvider(Now);
        return new McpGateway(new RegistryMcpPermissionEvaluator(registry, clock), events, clock, connectors);
    }

    private static McpGatewayRequest Request(
        AgentId agentId,
        McpConnectorKey connector,
        string operation,
        PermissionLevel permission,
        bool isDestructive = false,
        Approval? approval = null,
        IReadOnlyDictionary<string, string?>? arguments = null)
    {
        var missionId = new MissionId("mission-001");
        return new McpGatewayRequest(
            new ToolCall(
                new ToolCallId("call-001"),
                missionId,
                agentId,
                connector,
                operation,
                permission,
                RiskLevel.High,
                isDestructive,
                ToolCallState.Requested,
                Now,
                "corr-001",
                approval?.Id),
            Project,
            arguments ?? new Dictionary<string, string?>(),
            approval);
    }

    private static Approval Approved(string actionReference) =>
        new(
            new ApprovalId("approval-001"),
            new MissionId("mission-001"),
            Project,
            actionReference,
            ApprovalState.Approved,
            "forge",
            Now.AddMinutes(-5),
            "human-operator",
            Now.AddMinutes(-1),
            Now.AddMinutes(30));

    private sealed class StubConnector(McpConnectorKey key, bool shouldThrow = false) : IMcpConnector
    {
        public McpConnectorKey Key { get; } = key;

        public int Invocations { get; private set; }

        public ValueTask<McpCallResult> ExecuteAsync(
            McpGatewayRequest request,
            CancellationToken cancellationToken = default)
        {
            Invocations++;
            if (shouldThrow)
            {
                throw new InvalidOperationException("provider-secret-error");
            }

            return ValueTask.FromResult(new McpCallResult(
                request.ToolCall.Id,
                ExecutionOutcome.Succeeded,
                "Connector completed.",
                TimeSpan.Zero,
                "connector://result/1"));
        }
    }
}
