using MR.SAASy.Contracts.Access;
using MR.SAASy.Contracts.Context;

namespace MR.SAASy.Core.Context;

/// <summary>
/// Composes the access resolver and the context projection resolver into the single
/// fail-closed boundary described by WOR-574. Access is decided first; the field projection
/// plan is resolved only when the decision is <see cref="AccessGrantDecisionState.Granted"/>,
/// so a denied request never yields a field plan.
/// </summary>
public sealed class AgentContextGateway : IAgentContextGateway
{
    private readonly IAccessGrantResolver _accessResolver;
    private readonly IContextProjectionResolver _projectionResolver;

    public AgentContextGateway(
        IAccessGrantResolver accessResolver,
        IContextProjectionResolver projectionResolver)
    {
        _accessResolver = accessResolver ?? throw new ArgumentNullException(nameof(accessResolver));
        _projectionResolver = projectionResolver ?? throw new ArgumentNullException(nameof(projectionResolver));
    }

    public async ValueTask<AgentContextGrant> AuthorizeAsync(
        AgentContextRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var decision = await _accessResolver.ResolveAsync(
            request.IdentityId,
            request.Scope,
            request.Role,
            cancellationToken);

        if (!decision.IsGranted)
        {
            return new AgentContextGrant(decision, Projection: null);
        }

        var projection = _projectionResolver.Resolve(request.Capability, request.RequestedFields);
        return new AgentContextGrant(decision, projection);
    }
}
