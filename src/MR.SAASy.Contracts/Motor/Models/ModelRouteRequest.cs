using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Models;

public sealed record ModelRouteRequest(
    MissionId MissionId,
    MotorProjectContext Project,
    string CorrelationId,
    MissionTaskType TaskType,
    RiskLevel Risk,
    ComplexityLevel Complexity,
    bool IsBulk,
    bool IsLowValue,
    int EstimatedInputTokens,
    int EstimatedOutputTokens,
    IReadOnlyCollection<ModelCandidate> Candidates,
    IReadOnlyCollection<ModelPerformanceSnapshot> HistoricalPerformance);
