namespace MR.SAASy.Contracts.ControlCenter;

/// <summary>
/// Normalized health/readiness state for a control-plane observation. <see cref="Unknown"/>,
/// <see cref="Blocked"/> and <see cref="Stale"/> are first-class states and must never be coerced
/// to a healthy/successful value (ADR 0009). Provider-specific status strings are mapped into this
/// set by adapters before they reach the platform read model.
/// </summary>
public enum ObservationState
{
    /// <summary>No observation is available, or the source could not be reached.</summary>
    Unknown = 0,

    /// <summary>The application/environment is observed healthy and ready.</summary>
    Healthy = 1,

    /// <summary>Observed operational but degraded (partial failure, warnings, reduced capacity).</summary>
    Degraded = 2,

    /// <summary>Observed failing or not ready.</summary>
    Unhealthy = 3,

    /// <summary>Progress is blocked awaiting an external decision, approval or dependency.</summary>
    Blocked = 4,

    /// <summary>The observation exists but is too old to be trusted as current.</summary>
    Stale = 5
}
