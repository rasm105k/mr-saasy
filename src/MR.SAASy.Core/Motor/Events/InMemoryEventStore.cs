namespace MR.SAASy.Core.Motor.Events;

public sealed class InMemoryEventStore : IEventStore
{
    private readonly List<object> _events = new();

    public void Append(object @event)
    {
        _events.Add(@event);
    }

    public IReadOnlyCollection<object> ReadAll()
    {
        return _events.AsReadOnly();
    }
}
