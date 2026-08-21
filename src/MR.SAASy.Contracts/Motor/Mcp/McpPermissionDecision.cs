namespace MR.SAASy.Contracts.Motor.Mcp;

public sealed record McpPermissionDecision(
    bool IsAllowed,
    bool RequiresApproval,
    string Reason);
