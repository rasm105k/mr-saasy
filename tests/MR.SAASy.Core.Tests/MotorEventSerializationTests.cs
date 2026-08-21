using System.Text.Json;
using MR.SAASy.Contracts.Motor.Domain;
using MR.SAASy.Contracts.Motor.Events;
using MR.SAASy.Contracts.Motor.Mcp;
using MR.SAASy.Contracts.Motor.Models;
using MR.SAASy.Core.Motor.Events;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class MotorEventSerializationTests
{
    [Fact]
    public void Every_required_event_serializes_to_the_same_versioned_transport_shape()
    {
        var serializer = new SystemTextJsonMotorEventSerializer();

        foreach (var motorEvent in Events())
        {
            using var document = JsonDocument.Parse(serializer.Serialize(motorEvent));
            var root = document.RootElement;

            Assert.Equal(motorEvent.EventType, root.GetProperty("eventType").GetString());
            Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
            Assert.Equal("mission-001", root.GetProperty("missionId").GetString());
            Assert.Equal("workspace", root.GetProperty("workspaceId").GetString());
            Assert.Equal("project", root.GetProperty("projectId").GetString());
            Assert.Equal("corr-1", root.GetProperty("correlationId").GetString());
            Assert.Equal(JsonValueKind.Object, root.GetProperty("payload").ValueKind);
        }
    }

    [Fact]
    public void Event_envelope_does_not_require_provider_specific_types()
    {
        var serializer = new SystemTextJsonMotorEventSerializer();
        var envelope = serializer.CreateEnvelope(Events().OfType<ModelSelected>().Single());

        Assert.Equal("ModelSelected", envelope.EventType);
        Assert.Equal("project", envelope.ProjectId);
        Assert.Equal("opencode-zen", envelope.Payload.GetProperty("model").GetProperty("value").GetString());
    }

    private static IReadOnlyCollection<MotorEvent> Events()
    {
        var at = new DateTimeOffset(2026, 8, 21, 20, 0, 0, TimeSpan.Zero);
        var missionId = new MissionId("mission-001");
        var project = new MotorProjectContext("workspace", "project", "Project", "test", "customer");
        var agentId = new AgentId("forge");

        return
        [
            new MissionStarted(Id(1), missionId, project, at, "corr-1", "Build MOTOR", MissionTaskType.Code, RiskLevel.Medium),
            new AgentAssigned(Id(2), missionId, project, at, "corr-1", agentId, "Development", "Capability match"),
            new ModelSelected(Id(3), missionId, project, at, "corr-1", new ModelSelectionId("selection-1"), MotorModelKeys.OpenCodeZen, "Code route", 0.12m),
            new ToolCalled(Id(4), missionId, project, at, "corr-1", new ToolCallId("call-1"), agentId, KnownMcpConnectors.GitHub, "pull-request.create", ExecutionOutcome.Succeeded),
            new DecisionMade(Id(5), missionId, project, at, "corr-1", agentId, "architecture", "Use gateway", "docs://adr/0005"),
            new ActionSuggested(Id(6), missionId, project, at, "corr-1", agentId, "github.pull-request.merge", "Merge", RiskLevel.High, true),
            new ActionApproved(Id(7), missionId, project, at, "corr-1", new ApprovalId("approval-1"), "github.pull-request.merge", "human"),
            new ActionCompleted(Id(8), missionId, project, at, "corr-1", "github.pull-request.merge", agentId, ExecutionOutcome.Succeeded),
            new LearningCreated(Id(9), missionId, project, at, "corr-1", new LearningRecordId("learning-1"), agentId, "Accepted", ExecutionOutcome.Succeeded, true),
        ];
    }

    private static MotorEventId Id(int value) => new($"event-{value}");
}
