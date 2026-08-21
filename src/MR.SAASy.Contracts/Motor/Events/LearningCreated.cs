using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Events;

public sealed record LearningCreated(
    MotorEventId EventId,
    MissionId MissionId,
    MotorProjectContext Project,
    DateTimeOffset OccurredAt,
    string CorrelationId,
    LearningRecordId LearningRecordId,
    AgentId AgentId,
    string Summary,
    ExecutionOutcome Outcome,
    bool? HumanApproved,
    int SchemaVersion = 1)
    : MotorEvent(EventId, MissionId, Project, OccurredAt, CorrelationId, SchemaVersion)
{
    public override string EventType => nameof(LearningCreated);
}
