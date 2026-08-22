namespace MR.SAASy.Core.Motor.Workflow;

public interface IWorkflowEventPublisher
{
    Task PublishAsync(WorkflowEvent workflowEvent, CancellationToken cancellationToken = default);
}

public sealed class InMemoryWorkflowEventPublisher : IWorkflowEventPublisher
{
    private readonly List<WorkflowEvent> _events = [];

    public Task PublishAsync(WorkflowEvent workflowEvent, CancellationToken cancellationToken = default)
    {
        _events.Add(workflowEvent);
        return Task.CompletedTask;
    }

    public IReadOnlyCollection<WorkflowEvent> Events => _events;
}
