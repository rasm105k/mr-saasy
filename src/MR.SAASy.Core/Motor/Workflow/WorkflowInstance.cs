namespace MR.SAASy.Core.Motor.Workflow;

public sealed record WorkflowInstance(
    Guid Id,
    string MissionId,
    WorkflowState State,
    DateTimeOffset CreatedAt);
