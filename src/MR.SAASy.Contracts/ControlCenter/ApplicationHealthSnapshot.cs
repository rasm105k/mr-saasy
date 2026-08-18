using MR.SAASy.Contracts.Application;

namespace MR.SAASy.Contracts.ControlCenter;

/// <summary>
/// Normalized health/readiness observation for one application in one environment.
/// <see cref="ObservedAt"/> is when the source produced the observation; <see cref="RecordedAt"/> is
/// when the platform recorded it — the pair is what a freshness policy uses to decide staleness.
/// <see cref="RevisionReference"/> is the exact source revision (for example a commit sha) when the
/// provider supplies one.
/// </summary>
public sealed record ApplicationHealthSnapshot(
    ApplicationIdentifier ApplicationId,
    ApplicationEnvironment Environment,
    ObservationState State,
    DateTimeOffset ObservedAt,
    DateTimeOffset RecordedAt,
    string? RevisionReference = null,
    EvidenceReference? Evidence = null,
    string? Reason = null)
{
    public bool IsHealthy => State == ObservationState.Healthy;

    /// <summary>An explicitly unknown observation, used when no source reported state.</summary>
    public static ApplicationHealthSnapshot Unknown(
        ApplicationIdentifier applicationId,
        ApplicationEnvironment environment,
        DateTimeOffset recordedAt) =>
        new(applicationId, environment, ObservationState.Unknown, recordedAt, recordedAt);
}
