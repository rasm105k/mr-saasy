using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Models;

/// <summary>Historical aggregate supplied by memory; scores are expected in the 0..1 range.</summary>
public sealed record ModelPerformanceSnapshot(
    ModelKey Model,
    MissionTaskType TaskType,
    decimal SuccessRate,
    decimal QualityScore,
    decimal AverageActualCost,
    int SampleSize);
