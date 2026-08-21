using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Memory;

public sealed record BusinessImpactRecord(
    MemoryRecordId Id,
    MissionId MissionId,
    string Metric,
    decimal BaselineValue,
    decimal ObservedValue,
    string Unit,
    decimal Confidence,
    string EvidenceReference,
    DateTimeOffset RecordedAt);
