using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.ControlCenter;

namespace MR.SAASy.Core.ControlCenter;

/// <summary>
/// In-memory <see cref="IControlCenterReadModel"/> that aggregates over the registered
/// <see cref="IControlCenterProjectionSource"/> adapters, keyed by application. It performs no
/// provider ingestion and no mutation — it only fans out reads to sources, concurrently and in
/// isolation: a source that throws or is unreachable degrades to an explicit Unknown projection for
/// that application, so one broken product never collapses unrelated Control Center state. A query
/// for an application with no registered source likewise returns Unknown, never null and never a
/// healthy default. Freshness is a separate composable step (see
/// <see cref="ControlCenterFreshnessPolicy"/>); this aggregator does not impose a default window.
/// </summary>
public sealed class InMemoryControlCenterReadModel : IControlCenterReadModel
{
    private readonly IReadOnlyDictionary<ApplicationIdentifier, IControlCenterProjectionSource> _sources;
    private readonly TimeProvider _timeProvider;

    public InMemoryControlCenterReadModel(
        IEnumerable<IControlCenterProjectionSource> sources,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(sources);
        _timeProvider = timeProvider ?? TimeProvider.System;

        var byApplication = new Dictionary<ApplicationIdentifier, IControlCenterProjectionSource>();
        foreach (var source in sources)
        {
            ArgumentNullException.ThrowIfNull(source);
            byApplication[source.ApplicationId] = source;
        }

        _sources = byApplication;
    }

    public async ValueTask<ControlCenterProjection> GetApplicationAsync(
        ApplicationIdentifier applicationId,
        ApplicationEnvironment environment,
        CancellationToken cancellationToken = default) =>
        _sources.TryGetValue(applicationId, out var source)
            ? await ReadSafeAsync(source, environment, cancellationToken).ConfigureAwait(false)
            : ControlCenterProjection.Unknown(applicationId, environment, _timeProvider.GetUtcNow());

    public async ValueTask<IReadOnlyCollection<ControlCenterProjection>> GetAllAsync(
        ApplicationEnvironment environment,
        CancellationToken cancellationToken = default)
    {
        var reads = _sources.Values
            .Select(source => ReadSafeAsync(source, environment, cancellationToken).AsTask());

        return await Task.WhenAll(reads).ConfigureAwait(false);
    }

    // A source is an external adapter; a failure to read one must not fail the aggregate read.
    // Cancellation still propagates.
    private async ValueTask<ControlCenterProjection> ReadSafeAsync(
        IControlCenterProjectionSource source,
        ApplicationEnvironment environment,
        CancellationToken cancellationToken)
    {
        try
        {
            return await source.GetProjectionAsync(environment, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ControlCenterProjection.Unknown(source.ApplicationId, environment, _timeProvider.GetUtcNow());
        }
    }
}
