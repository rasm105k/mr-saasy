using MR.SAASy.Contracts.Identity;

namespace MR.SAASy.Contracts.Access;

public sealed record AccessGrantDecision(
    IdentityId IdentityId,
    AccessScope Scope,
    AccessRoleKey Role,
    AccessGrantDecisionState State,
    AccessGrantSource? Source = null,
    string? Reason = null)
{
    public bool IsGranted => State == AccessGrantDecisionState.Granted;
}
