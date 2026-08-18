using MR.SAASy.Contracts.Access;
using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Capabilities;
using MR.SAASy.Contracts.Context;
using MR.SAASy.Contracts.Identity;
using MR.SAASy.Contracts.Modules;
using MR.SAASy.Contracts.Modules.Entitlements;
using MR.SAASy.Contracts.Tenant;
using MR.SAASy.Core.Access;
using MR.SAASy.Core.Application;
using MR.SAASy.Core.Audit;
using MR.SAASy.Core.Capabilities;
using MR.SAASy.Core.Context;
using MR.SAASy.Core.Identity;
using MR.SAASy.Core.Modules;
using MR.SAASy.Core.Tenant;
using Xunit;

namespace MR.SAASy.Core.Tests;

/// <summary>
/// End-to-end composition: every platform surface wired from in-memory reference implementations,
/// with no product coupling. A registered application, an active bound tenant, an enabled capability
/// and a compatible module resolve as Enabled; an operator holding the matching grant is Granted a
/// minimized, masked context projection through the gateway. A request against an unbound tenant is
/// refused by both subsystems.
/// </summary>
public sealed class PlatformCompositionTests
{
    private static readonly ApplicationIdentifier Workslip = new("workslip");
    private static readonly TenantId TenantA = new("tenant-a");
    private static readonly TenantId TenantB = new("tenant-b");
    private static readonly ModuleId SupportModule = new("workslip.support");
    private static readonly CapabilityKey SupportCapability = new("workslip.support");
    private static readonly AccessRoleKey OperatorRole = new("platform.operator");
    private static readonly IdentityId Operator = new("id_human_operator");
    private static readonly ContextFieldKey DisplayName = new("display_name");
    private static readonly ContextFieldKey Email = new("email");
    private static readonly ContextFieldKey InternalNotes = new("internal_notes");
    private static readonly ModuleContractVersion HostContract = new("1.2.0");

    [Fact]
    public async Task Application_is_registered_for_its_environment()
    {
        var application = await ApplicationRegistry().GetAsync(Workslip, ApplicationEnvironment.Production);

        Assert.NotNull(application);
        Assert.Equal("Workslip", application.Name);
    }

    [Fact]
    public async Task Module_resolves_as_enabled_for_the_bound_tenant()
    {
        var resolver = new ModuleEntitlementResolver(TenantDirectory(), ModuleRegistry(), CapabilityRegistry());

        var decision = await resolver.ResolveAsync(
            new ModuleEntitlementQuery(TenantA, Workslip, SupportModule, HostContract));

        Assert.True(decision.IsEnabled);
        Assert.Equal(ModuleAvailabilityState.Enabled, decision.State);
    }

    [Fact]
    public async Task Operator_is_granted_a_minimized_and_masked_context_projection()
    {
        var audit = new InMemoryAuditSink();

        var result = await Gateway(audit).AuthorizeAsync(new AgentContextRequest(
            Operator,
            new AccessScope(AccessScopeKind.Tenant, Workslip, TenantA),
            OperatorRole,
            SupportCapability,
            new[] { DisplayName, Email, InternalNotes }));

        var projection = result.Projection;
        Assert.True(result.IsGranted);
        Assert.NotNull(projection);
        Assert.Equal(new[] { DisplayName, Email }, projection.GrantedFields);
        Assert.Equal(new[] { Email }, projection.MaskedFields);
        Assert.Equal(new[] { InternalNotes }, projection.DeniedFields);
        Assert.Equal(new[] { "context.request", "context.decision" }, audit.Events.Select(e => e.Name).ToArray());
    }

    [Fact]
    public async Task Request_for_an_unbound_tenant_is_refused_by_both_subsystems()
    {
        var moduleDecision = await new ModuleEntitlementResolver(TenantDirectory(), ModuleRegistry(), CapabilityRegistry())
            .ResolveAsync(new ModuleEntitlementQuery(TenantB, Workslip, SupportModule, HostContract));

        Assert.False(moduleDecision.IsEnabled);

        var access = await Gateway(new InMemoryAuditSink()).AuthorizeAsync(new AgentContextRequest(
            Operator,
            new AccessScope(AccessScopeKind.Tenant, Workslip, TenantB),
            OperatorRole,
            SupportCapability,
            new[] { DisplayName }));

        Assert.False(access.IsGranted);
        Assert.Null(access.Projection);
    }

    private static InMemoryApplicationRegistry ApplicationRegistry() =>
        new([new ApplicationDescriptor(Workslip, "Workslip", "1.0.0", ApplicationEnvironment.Production)]);

    private static InMemoryTenantDirectory TenantDirectory() =>
        new(
            [new TenantDescriptor(TenantA, "Tenant A", TenantLifecycleState.Active)],
            [new TenantApplicationBinding(TenantA, Workslip, null, TenantApplicationBindingState.Active)]);

    private static InMemoryCapabilityRegistry CapabilityRegistry() =>
        new(
            [new CapabilityDescriptor(SupportCapability, "Support")],
            [new CapabilityDecision(TenantA, Workslip, SupportCapability, CapabilityDecisionState.Enabled, CapabilityGrantSource.SystemPolicy)]);

    private static InMemoryModuleRegistry ModuleRegistry() =>
        new(
        [
            new ModuleManifest(
                SupportModule,
                "Workslip Support",
                new ModuleVersion("1.0.0"),
                new ModuleContractVersion("1.0.0"),
                Array.Empty<ModuleDependency>(),
                [new RequiredCapability(SupportCapability)],
                Array.Empty<ProvidedCapability>(),
                new ModuleCompatibility(new ModuleContractVersion("1.0.0"), new ModuleContractVersion("1.9.9"))),
        ]);

    private static AgentContextGateway Gateway(InMemoryAuditSink audit) =>
        new(
            new AccessGrantResolver(
                new InMemoryIdentityDirectory(
                [
                    new IdentityDescriptor(Operator, IdentityKind.Human, "Operator", IdentityLifecycleState.Active, Array.Empty<ExternalIdentitySubject>()),
                ]),
                new InMemoryAccessGrantStore(
                [
                    new AccessGrant(new AccessGrantId("g1"), Operator, new AccessScope(AccessScopeKind.Tenant, Workslip, TenantA), OperatorRole, AccessGrantSource.Manual),
                ]),
                TimeProvider.System),
            new ContextProjectionResolver(new CapabilityContextFieldPolicy(
                new Dictionary<CapabilityKey, CapabilityContextFieldPolicy.CapabilityFields>
                {
                    [SupportCapability] = new(Permitted: [DisplayName, Email], Masked: [Email]),
                })),
            audit);
}
