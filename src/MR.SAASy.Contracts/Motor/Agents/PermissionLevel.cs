namespace MR.SAASy.Contracts.Motor.Agents;

/// <summary>Ordered permission level. The default value grants nothing.</summary>
public enum PermissionLevel
{
    Denied = 0,
    Read = 1,
    Execute = 2,
    Write = 3,
    Delete = 4,
}
