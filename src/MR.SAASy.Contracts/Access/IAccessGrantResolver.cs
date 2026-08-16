using MR.SAASy.Contracts.Identity;

namespace MR.SAASy.Contracts.Access;

public interface IAccessGrantResolver
{
    ValueTask<AccessGrantDecision> ResolveAsync(
        IdentityId identityId,
        AccessScope scope,
        AccessRoleKey role,
        CancellationToken cancellationToken = default);
}
