namespace MR.SAASy.Contracts.Capabilities;

/// <summary>
/// Stable namespaced identifier for a platform capability.
/// Example: workslip.time-tracking.
/// </summary>
public readonly record struct CapabilityKey(string Value)
{
    public override string ToString() => Value;
}
