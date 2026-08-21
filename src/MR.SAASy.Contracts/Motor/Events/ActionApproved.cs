using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Events;

public sealed record ActionApproved(
    MotorEventId EventId,
    MissionId MissionId,
    MotorProjectContext Project,
    DateTimeOffset OccurredAt,
    string CorrelationId,
    ApprovalId ApprovalId,
    string ActionReference,
    string ApprovedBy,
    string? EvidenceReference = null,
    int SchemaVersion = 1)
    : MotorEvent(EventId, MissionId, Project, OccurredAt, CorrelationId, SchemaVersion)
{
    public override string EventType => nameof(ActionApproved);
}
