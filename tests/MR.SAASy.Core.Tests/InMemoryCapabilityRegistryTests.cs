using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Capabilities;
using MR.SAASy.Contracts.Tenant;
using MR.SAASy.Core.Capabilities;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class InMemoryCapabilityRegistryTests
{
    private static readonly TenantId TenantA = new("tenant-a");
    private static readonly ApplicationIdentifier Workslip = new("workslip");
    private static readonly CapabilityKey Support = new("workslip.support");

    [Fact]
    public async Task Finds_a_seeded_descriptor()
    {
        var registry = new InMemoryCapabilityRegistry(
            [new CapabilityDescriptor(Support, "Support")],
            []);

        var descriptor = await registry.FindAsync(Support);

        Assert.NotNull(descriptor);
        Assert.Equal("Support", descriptor.DisplayName);
    }

    [Fact]
    public async Task Resolves_a_seeded_enabled_decision()
    {
        var registry = new InMemoryCapabilityRegistry(
            [],
            [new CapabilityDecision(TenantA, Workslip, Support, CapabilityDecisionState.Enabled, CapabilityGrantSource.SystemPolicy)]);

        var decision = await registry.ResolveAsync(TenantA, Workslip, Support);

        Assert.True(decision.IsEnabled);
    }

    [Fact]
    public async Task Unseeded_target_resolves_to_unknown_and_not_enabled()
    {
        var registry = new InMemoryCapabilityRegistry([], []);

        var decision = await registry.ResolveAsync(TenantA, Workslip, Support);

        Assert.Equal(CapabilityDecisionState.Unknown, decision.State);
        Assert.False(decision.IsEnabled);
    }
}
