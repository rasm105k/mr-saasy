using MR.SAASy.Contracts.Motor.Agents;
using MR.SAASy.Contracts.Motor.Domain;
using MR.SAASy.Contracts.Motor.Events;
using MR.SAASy.Core.Motor.Agents;
using MR.SAASy.Core.Motor.Events;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class MotorAgentRegistryTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 21, 20, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Default_catalog_registers_the_six_named_specialists()
    {
        var registry = new InMemoryAgentRegistry(DefaultMotorAgentCatalog.Create());

        var agents = await registry.ListAsync();

        Assert.Equal(6, agents.Count);
        Assert.Equal(
            ["cleanup-guardian", "data-guardian", "forge", "gordon", "qa", "security-guardian"],
            agents.Select(agent => agent.Id.Value).OrderBy(id => id, StringComparer.Ordinal));
        Assert.All(agents, agent =>
        {
            Assert.NotEmpty(agent.Capabilities);
            Assert.NotEmpty(agent.Permissions);
            Assert.NotEmpty(agent.ApprovalRequirements);
            Assert.DoesNotContain(agent.Permissions, permission => permission.Level == PermissionLevel.Denied);
        });
    }

    [Fact]
    public async Task Gordon_is_read_only_by_default_and_has_no_cloud_write_grant()
    {
        var registry = new InMemoryAgentRegistry(DefaultMotorAgentCatalog.Create());

        var gordon = await registry.FindAsync(new AgentId("gordon"));

        Assert.NotNull(gordon);
        Assert.Equal(AgentAccessMode.ReadOnly, gordon.DefaultAccessMode);
        Assert.DoesNotContain(gordon.Permissions, permission => permission.Level >= PermissionLevel.Write);
    }

    [Fact]
    public async Task Forge_targets_opencode_zen_but_cannot_merge_without_approval()
    {
        var registry = new InMemoryAgentRegistry(DefaultMotorAgentCatalog.Create());

        var forge = await registry.FindAsync(new AgentId("forge"));

        Assert.NotNull(forge);
        Assert.Contains("opencode-zen", forge.Capabilities);
        Assert.Contains(forge.Permissions, permission =>
            permission.Operation == "pull-request.merge" && permission.RequiresApproval);
    }

    [Fact]
    public void Duplicate_agent_ids_are_rejected()
    {
        var agent = DefaultMotorAgentCatalog.Create().First();

        Assert.Throws<ArgumentException>(() => new InMemoryAgentRegistry([agent, agent]));
    }

    [Fact]
    public async Task Code_mission_routes_to_forge_and_emits_assignment_event()
    {
        var events = new InMemoryMotorEventSink();
        var router = new CapabilityAgentRouter(
            new InMemoryAgentRegistry(DefaultMotorAgentCatalog.Create()),
            events,
            new FixedTimeProvider(Now));

        var assignment = await router.AssignAsync(new AgentRouteRequest(
            Mission(MissionTaskType.Code),
            "corr-001"));

        Assert.Equal(new AgentId("forge"), assignment.AgentId);
        Assert.False(assignment.RequiresHumanReview);
        Assert.Equal(assignment.AgentId, Assert.IsType<AgentAssigned>(Assert.Single(events.Events)).AgentId);
    }

    [Fact]
    public async Task Unknown_mission_falls_back_to_qa_for_human_review()
    {
        var router = new CapabilityAgentRouter(
            new InMemoryAgentRegistry(DefaultMotorAgentCatalog.Create()),
            new InMemoryMotorEventSink(),
            new FixedTimeProvider(Now));

        var assignment = await router.AssignAsync(new AgentRouteRequest(
            Mission(MissionTaskType.Unknown),
            "corr-001"));

        Assert.Equal(new AgentId("qa"), assignment.AgentId);
        Assert.True(assignment.RequiresHumanReview);
    }

    private static Mission Mission(MissionTaskType taskType) =>
        new(
            new MissionId("mission-001"),
            new MotorProjectContext("workspace", "motor", "MOTOR", "test"),
            "Mission",
            "Route this work",
            taskType,
            RiskLevel.Medium,
            ComplexityLevel.Medium,
            MissionState.Planned,
            Now);
}
