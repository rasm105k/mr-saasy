namespace MR.SAASy.Contracts.Application;

public interface IApplicationRegistry
{
    ValueTask<ApplicationDescriptor?> GetAsync(
        ApplicationId applicationId,
        ApplicationEnvironment environment,
        CancellationToken cancellationToken = default);
}
