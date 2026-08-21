using MR.SAASy.Contracts.Motor.Domain;
using MR.SAASy.Contracts.Motor.Mcp;

namespace MR.SAASy.Contracts.Motor.Events;

public sealed record ToolCalled(
    MotorEventId EventId,
    MissionId MissionId,
    MotorProjectContext Project,
    DateTimeOffset OccurredAt,
    string CorrelationId,
    ToolCallId ToolCallId,
    AgentId AgentId,
    McpConnectorKey Connector,
    string Operation,
    ExecutionOutcome Outcome,
    string? ResultReference = null,
    int SchemaVersion = 1)
    : MotorEvent(EventId, MissionId, Project, OccurredAt, CorrelationId, SchemaVersion)
{
    public override string EventType => nameof(ToolCalled);
}
