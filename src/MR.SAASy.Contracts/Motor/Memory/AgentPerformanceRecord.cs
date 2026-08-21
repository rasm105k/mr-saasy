using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Memory;

public sealed record AgentPerformanceRecord(
    MemoryRecordId Id,
    AgentId AgentId,
    MissionTaskType TaskType,
    int CompletedMissions,
    decimal SuccessRate,
    decimal QualityScore,
    TimeSpan AverageDuration,
    DateTimeOffset RecordedAt);
