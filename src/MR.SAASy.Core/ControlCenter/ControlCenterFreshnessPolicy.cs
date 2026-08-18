using MR.SAASy.Contracts.ControlCenter;

namespace MR.SAASy.Core.ControlCenter;

/// <summary>
/// Pure staleness policy for control-plane observations. When a <em>healthy</em> observation is older
/// than <c>maxAge</c> it is reclassified as <see cref="ObservationState.Stale"/>, because an aged
/// healthy reading can no longer be trusted as current. Known-bad states
/// (<see cref="ObservationState.Unhealthy"/>, <see cref="ObservationState.Degraded"/>) keep their
/// signal — ageing must not soften a red into a milder "needs refresh". <see cref="ObservationState.Unknown"/>,
/// <see cref="ObservationState.Blocked"/> and <see cref="ObservationState.Stale"/> are first-class and
/// never reinterpreted (ADR 0009). The current time is passed in rather than read from an ambient
/// clock so the policy is deterministic. It never makes a stale observation look fresh.
/// </summary>
public static class ControlCenterFreshnessPolicy
{
    public static ApplicationHealthSnapshot ApplyStaleness(
        ApplicationHealthSnapshot snapshot,
        DateTimeOffset now,
        TimeSpan maxAge)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return HasAgedOutOfHealthy(snapshot.State, snapshot.ObservedAt, now, maxAge)
            ? snapshot with { State = ObservationState.Stale, Reason = AppendStaleReason(snapshot.Reason, maxAge) }
            : snapshot;
    }

    public static DeploymentEvidence ApplyStaleness(
        DeploymentEvidence deployment,
        DateTimeOffset now,
        TimeSpan maxAge)
    {
        ArgumentNullException.ThrowIfNull(deployment);

        return HasAgedOutOfHealthy(deployment.State, deployment.ObservedAt, now, maxAge)
            ? deployment with { State = ObservationState.Stale, Reason = AppendStaleReason(deployment.Reason, maxAge) }
            : deployment;
    }

    public static ControlCenterProjection ApplyStaleness(
        ControlCenterProjection projection,
        DateTimeOffset now,
        TimeSpan maxAge)
    {
        ArgumentNullException.ThrowIfNull(projection);

        return projection with
        {
            Health = ApplyStaleness(projection.Health, now, maxAge),
            LatestDeployment = projection.LatestDeployment is null
                ? null
                : ApplyStaleness(projection.LatestDeployment, now, maxAge)
        };
    }

    // Only a healthy observation ages into Stale. Non-healthy states are preserved: Unhealthy/Degraded
    // keep their known-bad signal, and Unknown/Blocked/Stale are first-class.
    private static bool HasAgedOutOfHealthy(
        ObservationState state,
        DateTimeOffset observedAt,
        DateTimeOffset now,
        TimeSpan maxAge) =>
        state == ObservationState.Healthy && now - observedAt > maxAge;

    private static string AppendStaleReason(string? reason, TimeSpan maxAge) =>
        reason is null
            ? $"Observation older than {maxAge}."
            : $"{reason} (observation older than {maxAge}).";
}
