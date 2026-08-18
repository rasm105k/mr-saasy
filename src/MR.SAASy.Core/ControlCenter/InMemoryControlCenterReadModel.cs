using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.ControlCenter;

namespace MR.SAASy.Core.ControlCenter;

/// <summary>
/// In-memory <see cref="IControlCenterReadModel"/> that aggregates over the registered
/// <see cref="IControlCenterProjectionSource"/> adapters, keyed by application. It performs no
/// provider ingestion and no mutation — it only fans out reads to sources. A query for an
/// application with no source returns an explicit Unknown projection, so one missing product never
/// collapses unrelated Control Center state.
/// </summary>
public sealed class InMemoryControlCenterReadModel : IControlCenterReadModel
{
    private readonly IReadOnlyDictionary<ApplicationIdentifier, IControlCenterProjectionSource> _sources;

    public InMemoryControlCenterReadModel(IEnumerable<IControlCenterProjectionSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);

        var byApplication = new Dictionary<ApplicationIdentifier, IControlCenterProjectionSource>();
        foreach (var source in sources)
        {
            ArgumentNullException.ThrowIfNull(source);
            byApplication[source.ApplicationId] = source;
        }

        _sources = byApplication;
    }

    public ValueTask<ControlCenterProjection> GetApplicationAsync(
        ApplicationIdentifier applicationId,
        ApplicationEnvironment environment,
        CancellationToken cancellationToken = default) =>
        _sources.TryGetValue(applicationId, out var source)
            ? source.GetProjectionAsync(environment, cancellationToken)
            : ValueTask.FromResult(
                ControlCenterProjection.Unknown(applicationId, environment, DateTimeOffset.UtcNow));

    public async ValueTask<IReadOnlyCollection<ControlCenterProjection>> GetAllAsync(
        ApplicationEnvironment environment,
        CancellationToken cancellationToken = default)
    {
        var projections = new List<ControlCenterProjection>(_sources.Count);
        foreach (var source in _sources.Values)
        {
            projections.Add(await source.GetProjectionAsync(environment, cancellationToken).ConfigureAwait(false));
        }

        return projections;
    }
}
