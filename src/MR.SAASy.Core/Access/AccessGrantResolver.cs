using MR.SAASy.Contracts.Access;
using MR.SAASy.Contracts.Identity;

namespace MR.SAASy.Core.Access;

/// <summary>
/// Provider-neutral, fail-closed access resolver.
/// It combines identity lifecycle, explicit grants, scope validation and grant expiry
/// into an <see cref="AccessGrantDecision"/> without importing product authorization
/// models or provider SDK types.
/// </summary>
/// <remarks>
/// Only an active identity holding a matching, unexpired grant for the exact requested
/// scope and role is <see cref="AccessGrantDecisionState.Granted"/>. There is no implicit
/// scope or role cascade: a Platform grant does not satisfy a Tenant request and a grant
/// for one tenant does not satisfy another, so tenants stay isolated by default. Absence
/// of a matching grant is <see cref="AccessGrantDecisionState.Denied"/>, an unregistered
/// identity is <see cref="AccessGrantDecisionState.Unknown"/>, and an invalid or incomplete
/// scope combination is <see cref="AccessGrantDecisionState.Unsupported"/>.
/// </remarks>
public sealed class AccessGrantResolver : IAccessGrantResolver
{
    private readonly IIdentityDirectory _identityDirectory;
    private readonly IAccessGrantStore _grantStore;
    private readonly TimeProvider _timeProvider;

    public AccessGrantResolver(
        IIdentityDirectory identityDirectory,
        IAccessGrantStore grantStore,
        TimeProvider timeProvider)
    {
        _identityDirectory = identityDirectory ?? throw new ArgumentNullException(nameof(identityDirectory));
        _grantStore = grantStore ?? throw new ArgumentNullException(nameof(grantStore));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async ValueTask<AccessGrantDecision> ResolveAsync(
        IdentityId identityId,
        AccessScope scope,
        AccessRoleKey role,
        CancellationToken cancellationToken = default)
    {
        // 1. Structural scope validation runs first: an invalid request is unsupported
        //    regardless of identity, and no identity/grant lookup should be provoked by it.
        if (!IsScopeComplete(scope, out var scopeReason))
        {
            return Decision(identityId, scope, role, AccessGrantDecisionState.Unsupported, reason: scopeReason);
        }

        // 2. Identity lifecycle. An identity we do not know is Unknown; a known but
        //    non-active identity is Denied.
        var identity = await _identityDirectory.FindAsync(identityId, cancellationToken);
        if (identity is null)
        {
            return Decision(identityId, scope, role, AccessGrantDecisionState.Unknown,
                reason: "Identity is not registered in MR SAAS'y.");
        }

        if (identity.LifecycleState != IdentityLifecycleState.Active)
        {
            return Decision(identityId, scope, role, AccessGrantDecisionState.Denied,
                reason: $"Identity lifecycle is {identity.LifecycleState}; Active is required.");
        }

        // 3. Explicit grants. An active identity with no matching, unexpired grant has
        //    zero access by default.
        var grants = await _grantStore.GetGrantsAsync(identityId, cancellationToken);
        var now = _timeProvider.GetUtcNow();

        AccessGrant? match = null;
        foreach (var grant in grants)
        {
            if (grant.Role != role)
            {
                continue;
            }

            if (!ScopeMatches(grant.Scope, scope))
            {
                continue;
            }

            if (grant.ExpiresAt is { } expiresAt && expiresAt <= now)
            {
                continue;
            }

            match = grant;
            break;
        }

        if (match is null)
        {
            return Decision(identityId, scope, role, AccessGrantDecisionState.Denied,
                reason: "No active, unexpired grant matches the requested scope and role.");
        }

        return Decision(identityId, scope, role, AccessGrantDecisionState.Granted, match.Source,
            "An active grant matches the requested scope and role.");
    }

    private static bool IsScopeComplete(AccessScope scope, out string reason)
    {
        switch (scope.Kind)
        {
            case AccessScopeKind.Platform:
                if (scope.ApplicationId is not null || scope.TenantId is not null || scope.Environment is not null)
                {
                    reason = "Platform scope must not specify an application, tenant or environment.";
                    return false;
                }

                break;

            case AccessScopeKind.Application:
                if (scope.ApplicationId is null)
                {
                    reason = "Application scope requires an application identifier.";
                    return false;
                }

                if (scope.TenantId is not null || scope.Environment is not null)
                {
                    reason = "Application scope must not specify a tenant or environment.";
                    return false;
                }

                break;

            case AccessScopeKind.Tenant:
                if (scope.ApplicationId is null || scope.TenantId is null)
                {
                    reason = "Tenant scope requires an application identifier and a tenant identifier.";
                    return false;
                }

                if (scope.Environment is not null)
                {
                    reason = "Tenant scope must not specify an environment.";
                    return false;
                }

                break;

            case AccessScopeKind.Environment:
                if (scope.ApplicationId is null || scope.Environment is null)
                {
                    reason = "Environment scope requires an application identifier and an environment.";
                    return false;
                }

                if (scope.TenantId is not null)
                {
                    reason = "Environment scope must not specify a tenant.";
                    return false;
                }

                break;

            default:
                reason = $"Unsupported access scope kind '{scope.Kind}'.";
                return false;
        }

        reason = string.Empty;
        return true;
    }

    private static bool ScopeMatches(AccessScope grantScope, AccessScope requestScope)
    {
        if (grantScope.Kind != requestScope.Kind)
        {
            return false;
        }

        return requestScope.Kind switch
        {
            AccessScopeKind.Platform => true,
            AccessScopeKind.Application => grantScope.ApplicationId == requestScope.ApplicationId,
            AccessScopeKind.Tenant => grantScope.ApplicationId == requestScope.ApplicationId
                && grantScope.TenantId == requestScope.TenantId,
            AccessScopeKind.Environment => grantScope.ApplicationId == requestScope.ApplicationId
                && grantScope.Environment == requestScope.Environment,
            _ => false,
        };
    }

    private static AccessGrantDecision Decision(
        IdentityId identityId,
        AccessScope scope,
        AccessRoleKey role,
        AccessGrantDecisionState state,
        AccessGrantSource? source = null,
        string? reason = null) =>
        new(identityId, scope, role, state, source, reason);
}
