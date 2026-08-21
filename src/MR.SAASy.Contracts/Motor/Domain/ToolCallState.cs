namespace MR.SAASy.Contracts.Motor.Domain;

public enum ToolCallState
{
    Requested = 0,
    PermissionDenied = 1,
    ApprovalRequired = 2,
    Running = 3,
    Completed = 4,
    Failed = 5,
}
