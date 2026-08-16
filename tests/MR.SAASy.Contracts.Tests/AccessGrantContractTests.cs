using MR.SAASy.Contracts.Access;
using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Identity;
using MR.SAASy.Contracts.Tenant;
using Xunit;

namespace MR.SAASy.Contracts.Tests;

public sealed class AccessGrantContractTests
{
    private static readonly IdentityId IdentityId = new("id_human_001");

    [Theory]
    [InlineData(AccessGrantDecisionState.Denied)]
    [InlineData(AccessGrantDecisionState.Unknown)]
    [InlineData(AccessGrantDecisionState.Unsupported)]
    public void Non_granted_states_fail_closed(AccessGrantDecisionState state)
    {
        var decision = new AccessGrantDecision(
            IdentityId,
            new AccessScope(AccessScopeKind.Platform),
            new AccessRoleKey("platform.superadmin"),
            state);

        Assert.False(decision.IsGranted);
    }

    [Fact]
    public void Granted_is_the_only_positive_access_decision()
    {
        var decision = new AccessGrantDecision(
            IdentityId,
            new AccessScope(AccessScopeKind.Platform),
            new AccessRoleKey("platform.operator"),
            AccessGrantDecisionState.Granted,
            AccessGrantSource.Policy);

        Assert.True(decision.IsGranted);
    }

    [Fact]
    public void Product_admin_role_is_not_platform_superadmin_role()
    {
        var productRole = new AccessRoleKey("workslip.admin");
        var platformRole = new AccessRoleKey("platform.superadmin");

        Assert.NotEqual(productRole, platformRole);
    }

    [Fact]
    public void Tenant_scope_is_explicitly_bound_to_application_and_tenant()
    {
        var applicationId = new ApplicationIdentifier("workslip");
        var tenantId = new TenantId("tenant-a");
        var scope = new AccessScope(
            AccessScopeKind.Tenant,
            applicationId,
            tenantId);

        Assert.Equal(AccessScopeKind.Tenant, scope.Kind);
        Assert.Equal(applicationId, scope.ApplicationId);
        Assert.Equal(tenantId, scope.TenantId);
    }

    [Fact]
    public void Grant_can_expire_without_changing_identity()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var grant = new AccessGrant(
            new AccessGrantId("grant-001"),
            IdentityId,
            new AccessScope(AccessScopeKind.Environment, new ApplicationIdentifier("workslip"), Environment: ApplicationEnvironment.Production),
            new AccessRoleKey("application.deployer"),
            AccessGrantSource.Manual,
            expiresAt);

        Assert.Equal(IdentityId, grant.IdentityId);
        Assert.Equal(expiresAt, grant.ExpiresAt);
    }
}
