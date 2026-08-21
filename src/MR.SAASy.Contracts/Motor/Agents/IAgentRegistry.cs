using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Agents;

public interface IAgentRegistry
{
    ValueTask<AgentDefinition?> FindAsync(
        AgentId agentId,
        CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyCollection<AgentDefinition>> ListAsync(
        CancellationToken cancellationToken = default);
}
