using MR.SAASy.Contracts.Application;

namespace MR.SAASy.Contracts.ControlCenter;

/// <summary>
/// The aggregated, read-only control-plane view of a single application in a single environment:
/// its normalized health, latest deployment evidence and most recent automation runs. This is the
/// provider-neutral shape a Control Center read model returns; it references sources of truth and
/// never becomes a second store of provider data (ADR 0009).
/// </summary>
public sealed record ControlCenterProjection(
    ApplicationIdentifier ApplicationId,
    ApplicationEnvironment Environment,
    ApplicationHealthSnapshot Health,
    DeploymentEvidence? LatestDeployment,
    IReadOnlyCollection<AutomationRun> RecentRuns)
{
    /// <summary>
    /// An explicitly unknown projection for an application/environment no source can report on.
    /// A missing source degrades to <see cref="ObservationState.Unknown"/> — it never collapses to
    /// null or to a healthy value.
    /// </summary>
    public static ControlCenterProjection Unknown(
        ApplicationIdentifier applicationId,
        ApplicationEnvironment environment,
        DateTimeOffset recordedAt) =>
        new(
            applicationId,
            environment,
            ApplicationHealthSnapshot.Unknown(applicationId, environment, recordedAt),
            LatestDeployment: null,
            RecentRuns: []);
}
