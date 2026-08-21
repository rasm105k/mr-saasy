using MR.SAASy.Contracts.Motor.Domain;
using MR.SAASy.Contracts.Motor.Memory;

namespace MR.SAASy.Core.Motor.Memory;

/// <summary>
/// Append-only reference memory for local development and tests. It is not a production
/// persistence implementation and performs no automatic model training.
/// </summary>
public sealed class InMemoryMotorMemoryStore : IMotorMemoryStore
{
    private readonly object _sync = new();
    private readonly List<DecisionMemory> _decisions = [];
    private readonly List<SolutionMemory> _solutions = [];
    private readonly List<AgentPerformanceRecord> _agentPerformance = [];
    private readonly List<ModelPerformanceRecord> _modelPerformance = [];
    private readonly List<BusinessImpactRecord> _businessImpact = [];
    private readonly List<LearningRecord> _learning = [];

    public ValueTask AddDecisionAsync(DecisionMemory record, CancellationToken cancellationToken = default) =>
        AddAsync(record, _decisions, item => item.Id);

    public ValueTask AddSolutionAsync(SolutionMemory record, CancellationToken cancellationToken = default) =>
        AddAsync(record, _solutions, item => item.Id);

    public ValueTask AddAgentPerformanceAsync(
        AgentPerformanceRecord record,
        CancellationToken cancellationToken = default) =>
        AddAsync(record, _agentPerformance, item => item.Id);

    public ValueTask AddModelPerformanceAsync(
        ModelPerformanceRecord record,
        CancellationToken cancellationToken = default) =>
        AddAsync(record, _modelPerformance, item => item.Id);

    public ValueTask AddBusinessImpactAsync(
        BusinessImpactRecord record,
        CancellationToken cancellationToken = default) =>
        AddAsync(record, _businessImpact, item => item.Id);

    public ValueTask AddLearningAsync(LearningRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        lock (_sync)
        {
            if (_learning.Any(item => item.Id == record.Id))
            {
                throw new InvalidOperationException($"Duplicate learning record id '{record.Id}'.");
            }

            _learning.Add(record);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask<MotorMemorySnapshot> SnapshotAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            return ValueTask.FromResult(new MotorMemorySnapshot(
                _decisions.ToArray(),
                _solutions.ToArray(),
                _agentPerformance.ToArray(),
                _modelPerformance.ToArray(),
                _businessImpact.ToArray(),
                _learning.ToArray()));
        }
    }

    private ValueTask AddAsync<T>(T record, List<T> target, Func<T, MemoryRecordId> idSelector)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(record);
        lock (_sync)
        {
            var id = idSelector(record);
            if (target.Any(item => idSelector(item) == id))
            {
                throw new InvalidOperationException($"Duplicate memory record id '{id}'.");
            }

            target.Add(record);
        }

        return ValueTask.CompletedTask;
    }
}
