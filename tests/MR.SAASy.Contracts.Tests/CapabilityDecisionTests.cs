using MR.SAASy.Contracts.Capabilities;
using MR.SAASy.Contracts.Tenant;
using Xunit;

namespace MR.SAASy.Contracts.Tests;

public sealed class CapabilityDecisionTests
{
    private static readonly TenantId Tenant = new("tenant-test");
    private static readonly CapabilityKey Capability = new("workslip.time-tracking");

    [Theory]
    [InlineData(CapabilityDecisionState.Disabled)]
    [InlineData(CapabilityDecisionState.Unknown)]
    [InlineData(CapabilityDecisionState.Unsupported)]
    public void Non_enabled_states_fail_closed(CapabilityDecisionState state)
    {
        var decision = new CapabilityDecision(
            Tenant,
            "workslip",
            Capability,
            state);

        Assert.False(decision.IsEnabled);
    }

    [Fact]
    public void Explicit_enabled_state_is_enabled()
    {
        var decision = new CapabilityDecision(
            Tenant,
            "workslip",
            Capability,
            CapabilityDecisionState.Enabled,
            CapabilityGrantSource.Subscription);

        Assert.True(decision.IsEnabled);
        Assert.Equal(CapabilityGrantSource.Subscription, decision.Source);
    }

    [Fact]
    public void Decision_is_scoped_to_tenant_application_and_capability()
    {
        var decision = new CapabilityDecision(
            new TenantId("tenant-a"),
            "workslip",
            new CapabilityKey("workslip.documents"),
            CapabilityDecisionState.Enabled,
            CapabilityGrantSource.Plan);

        Assert.Equal("tenant-a", decision.TenantId.Value);
        Assert.Equal("workslip", decision.ApplicationId);
        Assert.Equal("workslip.documents", decision.CapabilityKey.Value);
    }
}
