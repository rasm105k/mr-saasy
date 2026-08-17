using MR.SAASy.Contracts.Access;
using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Capabilities;
using MR.SAASy.Contracts.Context;
using MR.SAASy.Contracts.Identity;
using MR.SAASy.Contracts.Tenant;
using MR.SAASy.Core.Access;
using MR.SAASy.Core.Audit;
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
        var gateway = new AgentContextGateway(AccessResolver(identities: [], grants: []), projector, new InMemoryAuditSink());

        var result = await gateway.AuthorizeAsync(Request(new[] { DisplayName, Email }));

        Assert.False(result.IsGranted);
        Assert.Equal(AccessGrantDecisionState.Unknown, result.Decision.State);
        Assert.Null(result.Projection);
        Assert.Equal(0, projector.Calls);
    }

    [Fact]
    public async Task Granted_access_returns_the_minimized_and_masked_projection_plan()
    {
        var result = await GrantingGateway(out _).AuthorizeAsync(Request(new[] { DisplayName, Email, InternalNotes }));

        var projection = result.Projection;
        Assert.True(result.IsGranted);
        Assert.NotNull(projection);
        Assert.Equal(new[] { DisplayName, Email }, projection.GrantedFields);
        Assert.Equal(new[] { Email }, projection.MaskedFields);
        Assert.Equal(new[] { InternalNotes }, projection.DeniedFields);
    }

    [Fact]
    public async Task Emits_request_and_decision_audit_events_on_grant()
    {
        var gateway = GrantingGateway(out var audit);

        await gateway.AuthorizeAsync(Request(new[] { DisplayName, Email, InternalNotes }));

        Assert.Equal(new[] { "context.request", "context.decision" }, audit.Events.Select(e => e.Name).ToArray());

        var request = audit.Events[0].Metadata;
        Assert.Equal("id_human_operator", request["identity"]);
        Assert.Equal("Tenant", request["scope_kind"]);
        Assert.Equal("workslip", request["application"]);
        Assert.Equal("tenant-a", request["tenant"]);
        Assert.Equal("platform.operator", request["role"]);
        Assert.Equal("customer_support_summary", request["capability"]);
        Assert.Equal("display_name,email,internal_notes", request["requested_fields"]);
        Assert.Equal("3", request["requested_field_count"]);

        var decision = audit.Events[1].Metadata;
        Assert.Equal("Granted", decision["decision"]);
        Assert.Equal("display_name,email", decision["granted_fields"]);
        Assert.Equal("email", decision["masked_fields"]);
        Assert.Equal("internal_notes", decision["denied_fields"]);
    }

    [Fact]
    public async Task Emits_a_denied_decision_event_without_projection_fields()
    {
        var audit = new InMemoryAuditSink();
        var gateway = new AgentContextGateway(
            AccessResolver(identities: [], grants: []),
            new CountingProjectionResolver(),
            audit);

        await gateway.AuthorizeAsync(Request(new[] { DisplayName, Email }));

        Assert.Equal(new[] { "context.request", "context.decision" }, audit.Events.Select(e => e.Name).ToArray());

        var decision = audit.Events[1].Metadata;
        Assert.Equal("Unknown", decision["decision"]);
        Assert.Null(decision["granted_fields"]);
        Assert.Null(decision["masked_fields"]);
        Assert.Null(decision["denied_fields"]);
    }

    private static AgentContextGateway GrantingGateway(out InMemoryAuditSink audit)
    {
        audit = new InMemoryAuditSink();
        var scope = new AccessScope(AccessScopeKind.Tenant, Workslip, TenantA);
        return new AgentContextGateway(
            AccessResolver(
                identities: [ActiveOperator()],
                grants: [new AccessGrant(new AccessGrantId("g1"), Operator, scope, OperatorRole, AccessGrantSource.Manual)]),
            new ContextProjectionResolver(Policy()),
            audit);
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
