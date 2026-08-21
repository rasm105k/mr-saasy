using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Memory;

public sealed record MotorMemorySnapshot(
    IReadOnlyCollection<DecisionMemory> Decisions,
    IReadOnlyCollection<SolutionMemory> Solutions,
    IReadOnlyCollection<AgentPerformanceRecord> AgentPerformance,
    IReadOnlyCollection<ModelPerformanceRecord> ModelPerformance,
    IReadOnlyCollection<BusinessImpactRecord> BusinessImpact,
    IReadOnlyCollection<LearningRecord> Learning);
