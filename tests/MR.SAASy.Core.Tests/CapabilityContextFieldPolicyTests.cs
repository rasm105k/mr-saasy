using MR.SAASy.Contracts.Capabilities;
using MR.SAASy.Contracts.Context;
using MR.SAASy.Core.Context;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class CapabilityContextFieldPolicyTests
{
    private static readonly CapabilityKey Capability = new("customer_support_summary");
    private static readonly ContextFieldKey DisplayName = new("display_name");
    private static readonly ContextFieldKey Email = new("email");

    [Fact]
    public void Rejects_masking_a_field_that_is_not_permitted()
    {
        var config = new Dictionary<CapabilityKey, CapabilityContextFieldPolicy.CapabilityFields>
        {
            [Capability] = new(Permitted: [DisplayName], Masked: [Email]),
        };

        Assert.Throws<ArgumentException>(() => new CapabilityContextFieldPolicy(config));
    }

    [Fact]
    public void Unknown_capability_exposes_no_fields()
    {
        var policy = new CapabilityContextFieldPolicy(
            new Dictionary<CapabilityKey, CapabilityContextFieldPolicy.CapabilityFields>());

        Assert.False(policy.Knows(Capability));
        Assert.Empty(policy.PermittedFields(Capability));
        Assert.Empty(policy.MaskedFields(Capability));
    }

    [Fact]
    public void Exposes_configured_permitted_and_masked_fields()
    {
        var policy = new CapabilityContextFieldPolicy(
            new Dictionary<CapabilityKey, CapabilityContextFieldPolicy.CapabilityFields>
            {
                [Capability] = new(Permitted: [DisplayName, Email], Masked: [Email]),
            });

        Assert.True(policy.Knows(Capability));
        Assert.Equal(new[] { DisplayName, Email }, policy.PermittedFields(Capability));
        Assert.Equal(new[] { Email }, policy.MaskedFields(Capability));
    }
}
