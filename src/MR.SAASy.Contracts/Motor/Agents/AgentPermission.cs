namespace MR.SAASy.Contracts.Motor.Agents;

/// <summary>An explicit connector/operation grant; absence means denied.</summary>
public sealed record AgentPermission(
    string Connector,
    string Operation,
    PermissionLevel Level,
    bool RequiresApproval = false);
