namespace MR.SAASy.Core.Motor.Workflow;

public sealed class WorkflowStateMachine
{
    private static readonly IReadOnlyDictionary<WorkflowState, WorkflowState[]> AllowedTransitions =
        new Dictionary<WorkflowState, WorkflowState[]>
        {
            [WorkflowState.Created] = [WorkflowState.Running, WorkflowState.Cancelled],
            [WorkflowState.Running] = [WorkflowState.WaitingForApproval, WorkflowState.Completed, WorkflowState.Failed],
            [WorkflowState.WaitingForApproval] = [WorkflowState.Running, WorkflowState.Cancelled],
            [WorkflowState.Failed] = [WorkflowState.Running, WorkflowState.Cancelled],
            [WorkflowState.Completed] = [],
            [WorkflowState.Cancelled] = []
        };

    public bool CanTransition(WorkflowState current, WorkflowState next)
    {
        return AllowedTransitions.TryGetValue(current, out var transitions)
            && transitions.Contains(next);
    }

    public WorkflowState Transition(WorkflowState current, WorkflowState next)
    {
        if (!CanTransition(current, next))
            throw new InvalidOperationException($"Invalid workflow transition: {current} -> {next}");

        return next;
    }
}
