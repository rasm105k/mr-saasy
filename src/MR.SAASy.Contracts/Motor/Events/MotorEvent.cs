using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Events;

/// <summary>
/// Common, versioned event metadata suitable for later EventStoreDB, Data Lake,
/// Power BI and ML adapters. Payloads must contain references and safe metadata only.
/// </summary>
public abstract record MotorEvent(
    MotorEventId EventId,
    MissionId MissionId,
    MotorProjectContext Project,
    DateTimeOffset OccurredAt,
    string CorrelationId,
    int SchemaVersion)
{
    public abstract string EventType { get; }
}
