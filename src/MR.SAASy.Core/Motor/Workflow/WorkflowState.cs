namespace MR.SAASy.Core.Motor.Workflow;

public enum WorkflowState
{
    Created,
    Running,
    WaitingForApproval,
    Completed,
    Failed,
    Cancelled
}
