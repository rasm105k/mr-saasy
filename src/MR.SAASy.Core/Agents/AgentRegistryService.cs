using System.Collections.Concurrent;

namespace MR.SAASy.Core.Agents;

public sealed class AgentRegistryService
{
    private readonly ConcurrentDictionary<string, AgentRecord> _agents = new();

    public IReadOnlyCollection<AgentRecord> GetAgents() => _agents.Values.ToList();

    public AgentRecord Register(AgentRecord agent)
    {
        _agents[agent.Id] = agent;
        return agent;
    }

    public bool UpdateStatus(string agentId, AgentStatus status)
    {
        if (!_agents.TryGetValue(agentId, out var agent))
            return false;

        _agents[agentId] = agent with { Status = status, LastUpdatedUtc = DateTime.UtcNow };
        return true;
    }
}

public enum AgentStatus
{
    Planned,
    Running,
    Waiting,
    Blocked,
    Completed
}

public sealed record AgentRecord(
    string Id,
    string Name,
    string Purpose,
    AgentStatus Status,
    DateTime LastUpdatedUtc
);
