namespace MR.SAASy.Contracts.Capabilities;

/// <summary>
/// Capability decision states. The default (0) value is non-authorizing: only <see cref="Enabled"/>
/// enables a capability, and it is deliberately not the zero value so a default-initialized decision
/// fails closed.
/// </summary>
public enum CapabilityDecisionState
{
    Unknown = 0,
    Disabled = 1,
    Unsupported = 2,
    Enabled = 3
}
