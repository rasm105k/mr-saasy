using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.ControlCenter;

namespace MR.SAASy.Core.ControlCenter;

/// <summary>
/// Provider-neutral, in-memory <see cref="IControlCenterProjectionSource"/> seeded with fixed
/// projections keyed by environment. A platform default for local/dev and tests, and a stand-in for
/// a real product adapter; an environment with no seeded projection degrades to an explicit Unknown
/// projection rather than a healthy default.
/// </summary>
public sealed class StaticControlCenterProjectionSource : IControlCenterProjectionSource
{
    private readonly IReadOnlyDictionary<ApplicationEnvironment, ControlCenterProjection> _byEnvironment;
    private readonly TimeProvider _timeProvider;

    public ApplicationIdentifier ApplicationId { get; }

    public StaticControlCenterProjectionSource(
        ApplicationIdentifier applicationId,
        IEnumerable<ControlCenterProjection> projections,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(projections);
        ApplicationId = applicationId;
        _timeProvider = timeProvider ?? TimeProvider.System;

        var byEnvironment = new Dictionary<ApplicationEnvironment, ControlCenterProjection>();
        foreach (var projection in projections)
        {
            if (projection.ApplicationId != applicationId)
            {
                throw new ArgumentException(
                    $"Projection for '{projection.ApplicationId}' cannot be registered on a source for '{applicationId}'.",
                    nameof(projections));
            }

            byEnvironment[projection.Environment] = projection;
        }

        _byEnvironment = byEnvironment;
    }

    public ValueTask<ControlCenterProjection> GetProjectionAsync(
        ApplicationEnvironment environment,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(
            _byEnvironment.TryGetValue(environment, out var projection)
                ? projection
                : ControlCenterProjection.Unknown(ApplicationId, environment, _timeProvider.GetUtcNow()));
}
