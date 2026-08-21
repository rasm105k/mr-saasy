using MR.SAASy.Contracts.Motor.Domain;
using MR.SAASy.Contracts.Motor.Models;

namespace MR.SAASy.Contracts.Motor.Events;

public sealed record ModelSelected(
    MotorEventId EventId,
    MissionId MissionId,
    MotorProjectContext Project,
    DateTimeOffset OccurredAt,
    string CorrelationId,
    ModelSelectionId SelectionId,
    ModelKey Model,
    string Reason,
    decimal EstimatedCost,
    int SchemaVersion = 1)
    : MotorEvent(EventId, MissionId, Project, OccurredAt, CorrelationId, SchemaVersion)
{
    public override string EventType => nameof(ModelSelected);
}
