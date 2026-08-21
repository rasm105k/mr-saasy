namespace MR.SAASy.Contracts.ControlCenter;

/// <summary>
/// Normalized lifecycle state for an automation/CI run projection. As with
/// <see cref="ObservationState"/>, <see cref="Unknown"/> and <see cref="Blocked"/> are first-class
/// and must never be coerced to <see cref="Succeeded"/>.
/// </summary>
public enum AutomationRunState
{
    /// <summary>No run state is available from the source.</summary>
    Unknown = 0,

    /// <summary>The run is accepted but not yet executing.</summary>
    Queued = 1,

    /// <summary>The run is currently executing.</summary>
    Running = 2,

    /// <summary>The run completed successfully.</summary>
    Succeeded = 3,

    /// <summary>The run completed with a failure.</summary>
    Failed = 4,

    /// <summary>The run was cancelled before completion.</summary>
    Cancelled = 5,

    /// <summary>The run is waiting on an external approval or dependency.</summary>
    Blocked = 6
}
