using MR.SAASy.Contracts.Motor.Models;

namespace MR.SAASy.Core.Motor.Models;

/// <summary>Single-process reference log. Production sinks are added behind the contract.</summary>
public sealed class InMemoryModelSelectionLog : IModelSelectionLog
{
    private readonly object _sync = new();
    private readonly List<ModelSelection> _selections = [];
    private readonly List<ModelExecutionResult> _results = [];

    public IReadOnlyCollection<ModelSelection> Selections
    {
        get
        {
            lock (_sync)
            {
                return _selections.ToArray();
            }
        }
    }

    public IReadOnlyCollection<ModelExecutionResult> Results
    {
        get
        {
            lock (_sync)
            {
                return _results.ToArray();
            }
        }
    }

    public ValueTask RecordSelectionAsync(
        ModelSelection selection,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(selection);
        lock (_sync)
        {
            _selections.Add(selection);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask RecordResultAsync(
        ModelExecutionResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        lock (_sync)
        {
            if (_selections.All(selection => selection.Id != result.SelectionId))
            {
                throw new InvalidOperationException("A model result cannot be recorded before its selection.");
            }

            _results.Add(result);
        }

        return ValueTask.CompletedTask;
    }
}
