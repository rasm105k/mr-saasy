using MR.SAASy.Contracts.Access;
using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Capabilities;
using MR.SAASy.Contracts.Context;
using MR.SAASy.Contracts.Identity;
using MR.SAASy.Contracts.Tenant;
using MR.SAASy.Core.Access;
using MR.SAASy.Core.Context;
using MR.SAASy.Core.Identity;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class AgentContextGatewayTests
{
    private static readonly IdentityId Operator = new("id_human_operator");
    private static readonly ApplicationIdentifier Workslip = new("workslip");
    private static readonly TenantId TenantA = new("tenant-a");
    private static readonly AccessRoleKey OperatorRole = new("platform.operator");
    private static readonly CapabilityKey Capability = new("customer_support_summary");
    private static readonly ContextFieldKey DisplayName = new("display_name");
    private static readonly ContextFieldKey Email = new("email");
    private static readonly ContextFieldKey InternalNotes = new("internal_notes");

    [Fact]
    public async Task Denied_access_yields_no_projection_and_never_touches_the_projector()
    {
        var projector = new CountingProjectionResolver();
        // No identities seeded -> the access decision is Unknown (fail-closed).
        var gateway = new AgentContextGateway(AccessResolver(identities: [], grants: []), projector);

        var result = await gateway.AuthorizeAsync(Request(new[] { DisplayName, Email }));

        Assert.False(result.IsGranted);
        Assert.Equal(AccessGrantDecisionState.Unknown, result.Decision.State);
        Assert.Null(result.Projection);
        Assert.Equal(0, projector.Calls);
    }

    [Fact]
    public async Task Granted_access_returns_the_minimized_and_masked_projection_plan()
    {
        var scope = new AccessScope(AccessScopeKind.Tenant, Workslip, TenantA);
        var gateway = new AgentContextGateway(
            AccessResolver(
                identities: [ActiveOperator()],
                grants: [new AccessGrant(new AccessGrantId("g1"), Operator, scope, OperatorRole, AccessGrantSource.Manual)]),
            new ContextProjectionResolver(Policy()));

        var result = await gateway.AuthorizeAsync(Request(new[] { DisplayName, Email, InternalNotes }));

        var projection = result.Projection;
        Assert.True(result.IsGranted);
        Assert.NotNull(projection);
        Assert.Equal(new[] { DisplayName, Email }, projection.GrantedFields);
        Assert.Equal(new[] { Email }, projection.MaskedFields);
        Assert.Equal(new[] { InternalNotes }, projection.DeniedFields);
    }

    private static AgentContextRequest Request(IReadOnlyCollection<ContextFieldKey> fields) =>
        new(Operator, new AccessScope(AccessScopeKind.Tenant, Workslip, TenantA), OperatorRole, Capability, fields);

    private static AccessGrantResolver AccessResolver(
        IReadOnlyCollection<IdentityDescriptor> identities,
        IReadOnlyCollection<AccessGrant> grants) =>
        new(new InMemoryIdentityDirectory(identities), new InMemoryAccessGrantStore(grants), TimeProvider.System);

    private static IdentityDescriptor ActiveOperator() =>
        new(Operator, IdentityKind.Human, "Operator", IdentityLifecycleState.Active, Array.Empty<ExternalIdentitySubject>());

    private static CapabilityContextFieldPolicy Policy() =>
        new(new Dictionary<CapabilityKey, CapabilityContextFieldPolicy.CapabilityFields>
        {
            [Capability] = new(Permitted: [DisplayName, Email], Masked: [Email]),
        });

    private sealed class CountingProjectionResolver : IContextProjectionResolver
    {
        public int Calls { get; private set; }

        public ContextProjectionPlan Resolve(
            CapabilityKey capability,
            IReadOnlyCollection<ContextFieldKey> requestedFields)
        {
            Calls++;
            return new ContextProjectionPlan(
                capability,
                Array.Empty<ContextFieldKey>(),
                Array.Empty<ContextFieldKey>(),
                Array.Empty<ContextFieldKey>());
        }
    }
}
