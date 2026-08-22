namespace MR.SAASy.Core.Motor.Events;

public interface IEventStore
{
    Task AppendAsync(string streamId, object @event, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<object>> ReadAsync(string streamId, CancellationToken cancellationToken = default);
}
