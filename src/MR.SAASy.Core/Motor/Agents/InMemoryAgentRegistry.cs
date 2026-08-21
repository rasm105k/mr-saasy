using MR.SAASy.Contracts.Motor.Agents;
using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Core.Motor.Agents;

/// <summary>Provider-neutral reference registry for local composition and tests.</summary>
public sealed class InMemoryAgentRegistry : IAgentRegistry
{
    private readonly IReadOnlyDictionary<AgentId, AgentDefinition> _agents;

    public InMemoryAgentRegistry(IEnumerable<AgentDefinition> agents)
    {
        ArgumentNullException.ThrowIfNull(agents);

        var byId = new Dictionary<AgentId, AgentDefinition>();
        foreach (var agent in agents)
        {
            if (!byId.TryAdd(agent.Id, agent))
            {
                throw new ArgumentException($"Duplicate MOTOR agent id '{agent.Id}'.", nameof(agents));
            }
        }

        _agents = byId;
    }

    public ValueTask<AgentDefinition?> FindAsync(
        AgentId agentId,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(_agents.TryGetValue(agentId, out var agent) ? agent : null);

    public ValueTask<IReadOnlyCollection<AgentDefinition>> ListAsync(
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult<IReadOnlyCollection<AgentDefinition>>(
            _agents.Values.OrderBy(agent => agent.Name, StringComparer.Ordinal).ToArray());
}
