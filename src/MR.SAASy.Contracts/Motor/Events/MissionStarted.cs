using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Events;

public sealed record MissionStarted(
    MotorEventId EventId,
    MissionId MissionId,
    MotorProjectContext Project,
    DateTimeOffset OccurredAt,
    string CorrelationId,
    string Objective,
    MissionTaskType TaskType,
    RiskLevel Risk,
    int SchemaVersion = 1)
    : MotorEvent(EventId, MissionId, Project, OccurredAt, CorrelationId, SchemaVersion)
{
    public override string EventType => nameof(MissionStarted);
}
