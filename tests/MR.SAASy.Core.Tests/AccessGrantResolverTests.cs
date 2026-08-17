using MR.SAASy.Contracts.Access;
using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Identity;
using MR.SAASy.Contracts.Tenant;
using MR.SAASy.Core.Access;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class AccessGrantResolverTests
{
    private static readonly IdentityId Subject = new("id_human_001");
    private static readonly AccessRoleKey OperatorRole = new("platform.operator");
    private static readonly DateTimeOffset Now = new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Granted_when_active_identity_holds_matching_unexpired_grant()
    {
        var scope = new AccessScope(AccessScopeKind.Platform);
        var resolver = Resolver(
            identity: ActiveIdentity(),
            grants: [Grant(scope, OperatorRole, AccessGrantSource.Policy)]);

        var decision = await resolver.ResolveAsync(Subject, scope, OperatorRole);

        Assert.True(decision.IsGranted);
        Assert.Equal(AccessGrantDecisionState.Granted, decision.State);
        Assert.Equal(AccessGrantSource.Policy, decision.Source);
    }

    [Fact]
    public async Task Unknown_when_identity_is_not_registered()
    {
        var scope = new AccessScope(AccessScopeKind.Platform);
        var resolver = Resolver(identity: null, grants: []);

        var decision = await resolver.ResolveAsync(Subject, scope, OperatorRole);

        Assert.Equal(AccessGrantDecisionState.Unknown, decision.State);
        Assert.False(decision.IsGranted);
    }

    [Fact]
    public async Task Denied_when_identity_is_not_active()
    {
        var scope = new AccessScope(AccessScopeKind.Platform);
        var resolver = Resolver(
            identity: IdentityWith(IdentityLifecycleState.Suspended),
            grants: [Grant(scope, OperatorRole, AccessGrantSource.Policy)]);

        var decision = await resolver.ResolveAsync(Subject, scope, OperatorRole);

        Assert.Equal(AccessGrantDecisionState.Denied, decision.State);
    }

    [Fact]
    public async Task Denied_when_no_grant_matches()
    {
        var scope = new AccessScope(AccessScopeKind.Platform);
        var resolver = Resolver(identity: ActiveIdentity(), grants: []);

        var decision = await resolver.ResolveAsync(Subject, scope, OperatorRole);

        Assert.Equal(AccessGrantDecisionState.Denied, decision.State);
    }

    [Fact]
    public async Task Denied_when_matching_grant_has_expired()
    {
        var scope = new AccessScope(AccessScopeKind.Platform);
        var resolver = Resolver(
            identity: ActiveIdentity(),
            grants: [Grant(scope, OperatorRole, AccessGrantSource.Manual, expiresAt: Now.AddMinutes(-1))]);

        var decision = await resolver.ResolveAsync(Subject, scope, OperatorRole);

        Assert.Equal(AccessGrantDecisionState.Denied, decision.State);
    }

    [Fact]
    public async Task Granted_when_matching_grant_has_not_yet_expired()
    {
        var scope = new AccessScope(AccessScopeKind.Platform);
        var resolver = Resolver(
            identity: ActiveIdentity(),
            grants: [Grant(scope, OperatorRole, AccessGrantSource.Manual, expiresAt: Now.AddMinutes(1))]);

        var decision = await resolver.ResolveAsync(Subject, scope, OperatorRole);

        Assert.True(decision.IsGranted);
    }

    [Fact]
    public async Task Unsupported_when_tenant_scope_is_missing_its_tenant()
    {
        var scope = new AccessScope(AccessScopeKind.Tenant, new ApplicationIdentifier("workslip"));
        var resolver = Resolver(
            identity: ActiveIdentity(),
            grants: [Grant(new AccessScope(AccessScopeKind.Platform), OperatorRole, AccessGrantSource.Policy)]);

        var decision = await resolver.ResolveAsync(Subject, scope, OperatorRole);

        Assert.Equal(AccessGrantDecisionState.Unsupported, decision.State);
    }

    [Fact]
    public async Task Denied_when_grant_is_for_a_different_tenant()
    {
        var applicationId = new ApplicationIdentifier("workslip");
        var grantScope = new AccessScope(AccessScopeKind.Tenant, applicationId, new TenantId("tenant-a"));
        var requestScope = new AccessScope(AccessScopeKind.Tenant, applicationId, new TenantId("tenant-b"));
        var resolver = Resolver(
            identity: ActiveIdentity(),
            grants: [Grant(grantScope, OperatorRole, AccessGrantSource.Manual)]);

        var decision = await resolver.ResolveAsync(Subject, requestScope, OperatorRole);

        Assert.Equal(AccessGrantDecisionState.Denied, decision.State);
    }

    [Fact]
    public async Task Denied_when_grant_is_for_a_different_role()
    {
        var scope = new AccessScope(AccessScopeKind.Platform);
        var resolver = Resolver(
            identity: ActiveIdentity(),
            grants: [Grant(scope, new AccessRoleKey("platform.superadmin"), AccessGrantSource.Policy)]);

        var decision = await resolver.ResolveAsync(Subject, scope, OperatorRole);

        Assert.Equal(AccessGrantDecisionState.Denied, decision.State);
    }

    private static AccessGrantResolver Resolver(
        IdentityDescriptor? identity,
        IReadOnlyCollection<AccessGrant> grants,
        DateTimeOffset? now = null) =>
        new(
            new FakeIdentityDirectory(identity),
            new FakeAccessGrantStore(grants),
            new FixedTimeProvider(now ?? Now));

    private static IdentityDescriptor ActiveIdentity() => IdentityWith(IdentityLifecycleState.Active);

    private static IdentityDescriptor IdentityWith(IdentityLifecycleState state) =>
        new(Subject, IdentityKind.Human, "Test Human", state, Array.Empty<ExternalIdentitySubject>());

    private static AccessGrant Grant(
        AccessScope scope,
        AccessRoleKey role,
        AccessGrantSource source,
        DateTimeOffset? expiresAt = null) =>
        new(new AccessGrantId("grant-001"), Subject, scope, role, source, expiresAt);

    private sealed class FakeIdentityDirectory : IIdentityDirectory
    {
        private readonly IdentityDescriptor? _identity;

        public FakeIdentityDirectory(IdentityDescriptor? identity) => _identity = identity;

        public ValueTask<IdentityDescriptor?> FindAsync(
            IdentityId identityId,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(_identity is not null && _identity.IdentityId == identityId ? _identity : null);

        public ValueTask<IdentityDescriptor?> FindByExternalSubjectAsync(
            ExternalIdentitySubject subject,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult<IdentityDescriptor?>(null);
    }

    private sealed class FakeAccessGrantStore : IAccessGrantStore
    {
        private readonly IReadOnlyCollection<AccessGrant> _grants;

        public FakeAccessGrantStore(IReadOnlyCollection<AccessGrant> grants) => _grants = grants;

        public ValueTask<IReadOnlyCollection<AccessGrant>> GetGrantsAsync(
            IdentityId identityId,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(_grants);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;

        public FixedTimeProvider(DateTimeOffset now) => _now = now;

        public override DateTimeOffset GetUtcNow() => _now;
    }
}
