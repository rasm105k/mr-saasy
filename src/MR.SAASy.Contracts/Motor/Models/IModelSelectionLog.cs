namespace MR.SAASy.Contracts.Motor.Models;

public interface IModelSelectionLog
{
    ValueTask RecordSelectionAsync(
        ModelSelection selection,
        CancellationToken cancellationToken = default);

    ValueTask RecordResultAsync(
        ModelExecutionResult result,
        CancellationToken cancellationToken = default);
}
