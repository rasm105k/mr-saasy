using MR.SAASy.Contracts.Motor.Domain;
using MR.SAASy.Contracts.Motor.Models;

namespace MR.SAASy.Contracts.Motor.Memory;

public sealed record ModelPerformanceRecord(
    MemoryRecordId Id,
    ModelKey Model,
    MissionTaskType TaskType,
    int CompletedExecutions,
    decimal SuccessRate,
    decimal QualityScore,
    decimal AverageActualCost,
    TimeSpan AverageDuration,
    DateTimeOffset RecordedAt);
