using MR.SAASy.Contracts.Access;
using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Identity;
using MR.SAASy.Core.Access;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class InMemoryAccessGrantStoreTests
{
    private static readonly IdentityId Alice = new("id_human_alice");
    private static readonly IdentityId Bob = new("id_human_bob");

    [Fact]
    public async Task Returns_only_the_grants_belonging_to_the_identity()
    {
        var store = new InMemoryAccessGrantStore(
        [
            Grant(Alice, "grant-alice", new AccessScope(AccessScopeKind.Platform), "platform.operator"),
            Grant(Bob, "grant-bob", new AccessScope(AccessScopeKind.Application, new ApplicationIdentifier("workslip")), "application.deployer"),
        ]);

        var aliceGrants = await store.GetGrantsAsync(Alice);

        var only = Assert.Single(aliceGrants);
        Assert.Equal("grant-alice", only.GrantId.Value);
    }

    [Fact]
    public async Task Returns_empty_for_unknown_identity()
    {
        var store = new InMemoryAccessGrantStore([]);

        var grants = await store.GetGrantsAsync(Alice);

        Assert.Empty(grants);
    }

    [Fact]
    public async Task Keeps_multiple_grants_for_the_same_identity()
    {
        var store = new InMemoryAccessGrantStore(
        [
            Grant(Alice, "grant-1", new AccessScope(AccessScopeKind.Platform), "platform.operator"),
            Grant(Alice, "grant-2", new AccessScope(AccessScopeKind.Platform), "platform.superadmin"),
        ]);

        var grants = await store.GetGrantsAsync(Alice);

        Assert.Equal(2, grants.Count);
    }

    private static AccessGrant Grant(IdentityId identity, string grantId, AccessScope scope, string role) =>
        new(new AccessGrantId(grantId), identity, scope, new AccessRoleKey(role), AccessGrantSource.Manual);
}
