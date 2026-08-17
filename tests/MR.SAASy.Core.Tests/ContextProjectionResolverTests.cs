using MR.SAASy.Contracts.Capabilities;
using MR.SAASy.Contracts.Context;
using MR.SAASy.Core.Context;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class ContextProjectionResolverTests
{
    private static readonly CapabilityKey Capability = new("customer_support_summary");
    private static readonly ContextFieldKey DisplayName = new("display_name");
    private static readonly ContextFieldKey Plan = new("plan");
    private static readonly ContextFieldKey Email = new("email");
    private static readonly ContextFieldKey TicketHistory = new("ticket_history");
    private static readonly ContextFieldKey InternalNotes = new("internal_notes");

    [Fact]
    public void Grants_only_permitted_requested_fields_and_denies_the_rest()
    {
        var plan = Resolver().Resolve(Capability, new[] { DisplayName, Plan, InternalNotes });

        Assert.Equal(new[] { DisplayName, Plan }, plan.GrantedFields);
        Assert.Equal(new[] { InternalNotes }, plan.DeniedFields);
        Assert.Empty(plan.MaskedFields);
    }

    [Fact]
    public void Reports_masked_fields_as_a_subset_of_granted()
    {
        var plan = Resolver().Resolve(Capability, new[] { DisplayName, Email });

        Assert.Equal(new[] { DisplayName, Email }, plan.GrantedFields);
        Assert.Equal(new[] { Email }, plan.MaskedFields);
        Assert.Empty(plan.DeniedFields);
    }

    [Fact]
    public void Unknown_capability_grants_nothing_and_denies_all_requested()
    {
        var plan = Resolver().Resolve(new CapabilityKey("mystery"), new[] { DisplayName, Email });

        Assert.Empty(plan.GrantedFields);
        Assert.Empty(plan.MaskedFields);
        Assert.Equal(new[] { DisplayName, Email }, plan.DeniedFields);
    }

    [Fact]
    public void Deduplicates_repeated_requested_fields()
    {
        var plan = Resolver().Resolve(Capability, new[] { DisplayName, DisplayName, Plan });

        Assert.Equal(new[] { DisplayName, Plan }, plan.GrantedFields);
    }

    [Fact]
    public void Empty_request_grants_and_denies_nothing()
    {
        var plan = Resolver().Resolve(Capability, Array.Empty<ContextFieldKey>());

        Assert.Empty(plan.GrantedFields);
        Assert.Empty(plan.DeniedFields);
    }

    private static ContextProjectionResolver Resolver() =>
        new(new CapabilityContextFieldPolicy(
            new Dictionary<CapabilityKey, CapabilityContextFieldPolicy.CapabilityFields>
            {
                [Capability] = new(
                    Permitted: [DisplayName, Plan, Email, TicketHistory],
                    Masked: [Email]),
            }));
}
