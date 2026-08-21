using MR.SAASy.Contracts.Motor.Domain;
using MR.SAASy.Contracts.Motor.Events;
using MR.SAASy.Contracts.Motor.Mcp;
using MR.SAASy.Contracts.Motor.Models;
using Xunit;

namespace MR.SAASy.Contracts.Tests;

public sealed class MotorEventContractTests
{
    [Fact]
    public void Motor_001_defines_all_required_versioned_events()
    {
        var at = new DateTimeOffset(2026, 8, 21, 20, 0, 0, TimeSpan.Zero);
        var missionId = new MissionId("mission-001");
        var project = new MotorProjectContext("workspace", "project", "Project", "test");
        var agentId = new AgentId("forge");
        MotorEvent[] events =
        [
            new MissionStarted(Id(1), missionId, project, at, "corr-1", "Build MOTOR", MissionTaskType.Code, RiskLevel.Medium),
            new AgentAssigned(Id(2), missionId, project, at, "corr-1", agentId, "Development", "Best capability match"),
            new ModelSelected(Id(3), missionId, project, at, "corr-1", new ModelSelectionId("selection-1"), MotorModelKeys.OpenCodeZen, "Code route", 0.12m),
            new ToolCalled(Id(4), missionId, project, at, "corr-1", new ToolCallId("call-1"), agentId, KnownMcpConnectors.GitHub, "pull-request.create", ExecutionOutcome.Succeeded, "github://pull/1"),
            new DecisionMade(Id(5), missionId, project, at, "corr-1", agentId, "architecture", "Use a gateway", "docs://adr/0005"),
            new ActionSuggested(Id(6), missionId, project, at, "corr-1", agentId, "github.pull-request.merge", "Merge the reviewed change", RiskLevel.High, true),
            new ActionApproved(Id(7), missionId, project, at, "corr-1", new ApprovalId("approval-1"), "github.pull-request.merge", "human-operator"),
            new ActionCompleted(Id(8), missionId, project, at, "corr-1", "github.pull-request.merge", agentId, ExecutionOutcome.Succeeded, "github://pull/1"),
            new LearningCreated(Id(9), missionId, project, at, "corr-1", new LearningRecordId("learning-1"), agentId, "Routing was accepted", ExecutionOutcome.Succeeded, true),
        ];

        Assert.Equal(9, events.Length);
        Assert.All(events, motorEvent =>
        {
            Assert.Equal(1, motorEvent.SchemaVersion);
            Assert.Equal(missionId, motorEvent.MissionId);
            Assert.False(string.IsNullOrWhiteSpace(motorEvent.EventType));
        });
        Assert.Equal(9, events.Select(motorEvent => motorEvent.EventType).Distinct().Count());
    }

    private static MotorEventId Id(int value) => new($"event-{value}");
}
