using MR.SAASy.Contracts.Access;

namespace MR.SAASy.Contracts.Context;

/// <summary>
/// The gateway's combined outcome: the access <paramref name="Decision"/>, plus the field
/// <paramref name="Projection"/> plan when — and only when — access was granted. On any
/// non-granted decision the projection is <see langword="null"/> and no fields are exposed.
/// </summary>
public sealed record AgentContextGrant(
    AccessGrantDecision Decision,
    ContextProjectionPlan? Projection)
{
    public bool IsGranted => Decision.IsGranted;
}
