using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Memory;

public interface IMotorMemoryStore
{
    ValueTask AddDecisionAsync(DecisionMemory record, CancellationToken cancellationToken = default);

    ValueTask AddSolutionAsync(SolutionMemory record, CancellationToken cancellationToken = default);

    ValueTask AddAgentPerformanceAsync(AgentPerformanceRecord record, CancellationToken cancellationToken = default);

    ValueTask AddModelPerformanceAsync(ModelPerformanceRecord record, CancellationToken cancellationToken = default);

    ValueTask AddBusinessImpactAsync(BusinessImpactRecord record, CancellationToken cancellationToken = default);

    ValueTask AddLearningAsync(LearningRecord record, CancellationToken cancellationToken = default);

    ValueTask<MotorMemorySnapshot> SnapshotAsync(CancellationToken cancellationToken = default);
}
