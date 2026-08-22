namespace MR.SAASy.Core.Motor.Workflow;

public sealed record WorkflowEvent(
    Guid WorkflowId,
    string EventType,
    DateTime OccurredAt,
    string? Description = null);
