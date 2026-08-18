using MR.SAASy.Contracts.ControlCenter;

namespace MR.SAASy.Core.ControlCenter;

/// <summary>
/// Pure staleness policy for control-plane observations. When an observation is older than
/// <c>maxAge</c> its health is reclassified as <see cref="ObservationState.Stale"/>. The current
/// time is passed in rather than read from an ambient clock so the policy is deterministic. It never
/// coerces <see cref="ObservationState.Unknown"/> or <see cref="ObservationState.Blocked"/> into a
/// healthy value, and it never makes a stale observation look fresh (ADR 0009).
/// </summary>
public static class ControlCenterFreshnessPolicy
{
    public static ApplicationHealthSnapshot ApplyStaleness(
        ApplicationHealthSnapshot snapshot,
        DateTimeOffset now,
        TimeSpan maxAge)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        // Unknown/Blocked/Stale are first-class and must not be reinterpreted as anything else.
        if (snapshot.State is ObservationState.Unknown
            or ObservationState.Blocked
            or ObservationState.Stale)
        {
            return snapshot;
        }

        if (now - snapshot.ObservedAt <= maxAge)
        {
            return snapshot;
        }

        return snapshot with
        {
            State = ObservationState.Stale,
            Reason = snapshot.Reason is null
                ? $"Observation older than {maxAge}."
                : $"{snapshot.Reason} (observation older than {maxAge})."
        };
    }

    public static ControlCenterProjection ApplyStaleness(
        ControlCenterProjection projection,
        DateTimeOffset now,
        TimeSpan maxAge)
    {
        ArgumentNullException.ThrowIfNull(projection);

        return projection with { Health = ApplyStaleness(projection.Health, now, maxAge) };
    }
}
