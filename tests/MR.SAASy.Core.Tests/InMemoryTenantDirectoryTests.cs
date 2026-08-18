using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Tenant;
using MR.SAASy.Core.Tenant;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class InMemoryTenantDirectoryTests
{
    private static readonly TenantId TenantA = new("tenant-a");
    private static readonly ApplicationIdentifier Workslip = new("workslip");

    [Fact]
    public async Task Finds_a_seeded_tenant()
    {
        var directory = new InMemoryTenantDirectory(
            [new TenantDescriptor(TenantA, "Tenant A", TenantLifecycleState.Active)],
            []);

        var tenant = await directory.FindAsync(TenantA);

        Assert.NotNull(tenant);
        Assert.Equal(TenantLifecycleState.Active, tenant.LifecycleState);
    }

    [Fact]
    public async Task Returns_null_for_unknown_tenant()
    {
        var directory = new InMemoryTenantDirectory([], []);

        Assert.Null(await directory.FindAsync(TenantA));
    }

    [Fact]
    public async Task Returns_only_the_bindings_for_the_requested_tenant()
    {
        var directory = new InMemoryTenantDirectory(
            [new TenantDescriptor(TenantA, "Tenant A", TenantLifecycleState.Active)],
            [new TenantApplicationBinding(TenantA, Workslip, null, TenantApplicationBindingState.Active)]);

        var bindings = await directory.GetApplicationBindingsAsync(TenantA);

        var binding = Assert.Single(bindings);
        Assert.Equal(Workslip, binding.ApplicationId);
        Assert.Equal(TenantApplicationBindingState.Active, binding.State);
    }

    [Fact]
    public async Task Returns_no_bindings_for_unknown_tenant()
    {
        var directory = new InMemoryTenantDirectory([], []);

        Assert.Empty(await directory.GetApplicationBindingsAsync(TenantA));
    }
}
