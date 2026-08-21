namespace MR.SAASy.Contracts.Motor.Agents;

public interface IAgentRouter
{
    ValueTask<AgentAssignment> AssignAsync(
        AgentRouteRequest request,
        CancellationToken cancellationToken = default);
}
