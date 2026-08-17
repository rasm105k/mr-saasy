using MR.SAASy.Contracts.Access;
using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Identity;
using MR.SAASy.Contracts.Tenant;
using MR.SAASy.Core.Access;
using MR.SAASy.Core.Identity;
using Xunit;

namespace MR.SAASy.Core.Tests;

/// <summary>
/// Wires <see cref="AccessGrantResolver"/> with the in-memory reference implementations end to end.
/// </summary>
public sealed class AccessGrantResolverIntegrationTests
{
    private static readonly IdentityId Operator = new("id_human_operator");
    private static readonly ApplicationIdentifier Workslip = new("workslip");
    private static readonly AccessRoleKey OperatorRole = new("platform.operator");

    [Fact]
    public async Task Grants_when_seeded_identity_holds_the_exact_tenant_grant()
    {
        var scope = new AccessScope(AccessScopeKind.Tenant, Workslip, new TenantId("tenant-a"));
        var resolver = Resolver(
            identities: [ActiveOperator()],
            grants: [Grant(scope)]);

        var decision = await resolver.ResolveAsync(Operator, scope, OperatorRole);

        Assert.True(decision.IsGranted);
        Assert.Equal(AccessGrantSource.Manual, decision.Source);
    }

    [Fact]
    public async Task Denies_when_request_targets_a_different_tenant()
    {
        var grantScope = new AccessScope(AccessScopeKind.Tenant, Workslip, new TenantId("tenant-a"));
        var requestScope = new AccessScope(AccessScopeKind.Tenant, Workslip, new TenantId("tenant-b"));
        var resolver = Resolver(
            identities: [ActiveOperator()],
            grants: [Grant(grantScope)]);

        var decision = await resolver.ResolveAsync(Operator, requestScope, OperatorRole);

        Assert.Equal(AccessGrantDecisionState.Denied, decision.State);
    }

    [Fact]
    public async Task Unknown_when_identity_was_never_seeded()
    {
        var scope = new AccessScope(AccessScopeKind.Platform);
        var resolver = Resolver(identities: [], grants: []);

        var decision = await resolver.ResolveAsync(Operator, scope, OperatorRole);

        Assert.Equal(AccessGrantDecisionState.Unknown, decision.State);
    }

    private static AccessGrantResolver Resolver(
        IReadOnlyCollection<IdentityDescriptor> identities,
        IReadOnlyCollection<AccessGrant> grants) =>
        new(
            new InMemoryIdentityDirectory(identities),
            new InMemoryAccessGrantStore(grants),
            TimeProvider.System);

    private static IdentityDescriptor ActiveOperator() =>
        new(Operator, IdentityKind.Human, "Operator", IdentityLifecycleState.Active, Array.Empty<ExternalIdentitySubject>());

    private static AccessGrant Grant(AccessScope scope) =>
        new(new AccessGrantId("g1"), Operator, scope, OperatorRole, AccessGrantSource.Manual);
}
