namespace MR.SAASy.Contracts.Access;

/// <summary>
/// Access decision states. The default (0) value is non-authorizing: only <see cref="Granted"/>
/// authorizes access, and it is deliberately not the zero value so a default-initialized decision
/// fails closed.
/// </summary>
public enum AccessGrantDecisionState
{
    Unknown = 0,
    Denied = 1,
    Unsupported = 2,
    Granted = 3
}
