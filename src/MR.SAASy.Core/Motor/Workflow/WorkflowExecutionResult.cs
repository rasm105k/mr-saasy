namespace MR.SAASy.Core.Motor.Workflow;

public sealed record WorkflowExecutionResult(
    Guid WorkflowId,
    bool Success,
    string? FailureReason = null);
