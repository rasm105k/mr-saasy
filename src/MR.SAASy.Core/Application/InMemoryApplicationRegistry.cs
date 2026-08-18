using MR.SAASy.Contracts.Application;

namespace MR.SAASy.Core.Application;

/// <summary>
/// Provider-neutral, in-memory <see cref="IApplicationRegistry"/> seeded from explicit application
/// descriptors, keyed by (application, environment). A platform default for local/dev and
/// integration tests; a descriptor is returned only for the exact environment it was registered for.
/// </summary>
public sealed class InMemoryApplicationRegistry : IApplicationRegistry
{
    private readonly IReadOnlyDictionary<(ApplicationIdentifier Application, ApplicationEnvironment Environment), ApplicationDescriptor> _applications;

    public InMemoryApplicationRegistry(IEnumerable<ApplicationDescriptor> applications)
    {
        ArgumentNullException.ThrowIfNull(applications);

        var byKey = new Dictionary<(ApplicationIdentifier, ApplicationEnvironment), ApplicationDescriptor>();
        foreach (var application in applications)
        {
            byKey[(application.ApplicationId, application.Environment)] = application;
        }

        _applications = byKey;
    }

    public ValueTask<ApplicationDescriptor?> GetAsync(
        ApplicationIdentifier applicationId,
        ApplicationEnvironment environment,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(
            _applications.TryGetValue((applicationId, environment), out var application) ? application : null);
}
