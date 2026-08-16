namespace MR.SAASy.Contracts.Application;

public interface IApplicationRegistry
{
    ValueTask<ApplicationDescriptor?> GetAsync(
        string applicationId,
        ApplicationEnvironment environment,
        CancellationToken cancellationToken = default);
}
