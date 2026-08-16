using MR.SAASy.Contracts.Identity;

namespace MR.SAASy.Contracts.Access;

public sealed record AccessGrant(
    AccessGrantId GrantId,
    IdentityId IdentityId,
    AccessScope Scope,
    AccessRoleKey Role,
    AccessGrantSource Source,
    DateTimeOffset? ExpiresAt = null);
