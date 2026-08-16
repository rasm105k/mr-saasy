namespace MR.SAASy.Contracts.Application;

public interface IApplicationRegistry
{
    ValueTask<ApplicationDescriptor?> GetAsync(
        ApplicationIdentifier applicationId,
        ApplicationEnvironment environment,
        CancellationToken cancellationToken = default);
}
