namespace MR.SAASy.Contracts.Context;

/// <summary>
/// The single boundary an agent crosses to read product context. It authorizes the request
/// and, only on a granted access decision, resolves the minimization/masking projection plan.
/// </summary>
public interface IAgentContextGateway
{
    ValueTask<AgentContextGrant> AuthorizeAsync(
        AgentContextRequest request,
        CancellationToken cancellationToken = default);
}
