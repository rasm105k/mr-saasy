using System.Collections.Concurrent;

namespace MR.SAASy.Core.Agents;

public sealed class AgentActivityStore
{
    private readonly ConcurrentQueue<AgentActivityEvent> _events = new();

    public void Add(AgentActivityEvent activityEvent)
    {
        _events.Enqueue(activityEvent);
    }

    public IReadOnlyCollection<AgentActivityEvent> GetHistory()
    {
        return _events.ToArray();
    }
}

public sealed record AgentActivityEvent(
    string AgentId,
    string EventType,
    string Description,
    DateTime CreatedUtc
);
