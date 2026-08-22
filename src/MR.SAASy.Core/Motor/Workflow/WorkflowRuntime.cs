namespace MR.SAASy.Core.Motor.Workflow;

public sealed class WorkflowRuntime
{
    private readonly IWorkflowEventPublisher _publisher;

    public WorkflowRuntime(IWorkflowEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task StartAsync(WorkflowInstance instance, CancellationToken cancellationToken = default)
    {
        await _publisher.PublishAsync(
            new WorkflowEvent(
                instance.Id,
                "WorkflowStarted",
                DateTime.UtcNow),
            cancellationToken);
    }
}
