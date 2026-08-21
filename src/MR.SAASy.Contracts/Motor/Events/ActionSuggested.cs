using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Events;

public sealed record ActionSuggested(
    MotorEventId EventId,
    MissionId MissionId,
    MotorProjectContext Project,
    DateTimeOffset OccurredAt,
    string CorrelationId,
    AgentId AgentId,
    string ActionReference,
    string Description,
    RiskLevel Risk,
    bool RequiresApproval,
    int SchemaVersion = 1)
    : MotorEvent(EventId, MissionId, Project, OccurredAt, CorrelationId, SchemaVersion)
{
    public override string EventType => nameof(ActionSuggested);
}
