namespace MR.SAASy.Contracts.Motor.Models;

public interface IModelRouter
{
    ValueTask<ModelSelection> SelectAsync(
        ModelRouteRequest request,
        CancellationToken cancellationToken = default);
}
