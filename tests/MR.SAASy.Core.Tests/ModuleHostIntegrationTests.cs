using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Capabilities;
using MR.SAASy.Contracts.Modules;
using MR.SAASy.Contracts.Modules.Entitlements;
using MR.SAASy.Contracts.Modules.Hosting;
using MR.SAASy.Contracts.Tenant;
using MR.SAASy.Core.Modules.Hosting;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class ModuleHostIntegrationTests
{
    private static readonly TenantId TenantId = new("tenant-a");
    private static readonly ApplicationIdentifier ApplicationId = new("workslip");
    private static readonly ModuleContractVersion HostContractVersion = new("1.2.0");

    [Fact]
    public async Task Composer_projects_only_enabled_modules_and_sorts_navigation()
    {
        var documents = new ModuleId("workslip.documents");
        var analytics = new ModuleId("workslip.analytics");
        var resolver = new FakeEntitlementResolver(new Dictionary<ModuleId, ModuleAvailabilityState>
        {
            [documents] = ModuleAvailabilityState.Enabled,
            [analytics] = ModuleAvailabilityState.BlockedCapability
        });

        var host = new ModuleHostDefinition(
            ApplicationId,
            HostContractVersion,
            [
                new ModuleHostRegistration(
                    documents,
                    [
                        new ModuleNavigationDescriptor("documents.recent", "Recent documents", "/documents/recent", 20),
                        new ModuleNavigationDescriptor("documents.all", "Documents", "/documents", 10)
                    ]),
                new ModuleHostRegistration(
                    analytics,
                    [new ModuleNavigationDescriptor("analytics", "Analytics", "/analytics", 5)])
            ]);

        var snapshot = await new ModuleHostComposer(resolver).ComposeAsync(TenantId, host);

        Assert.Equal([documents], snapshot.EnabledModules);
        Assert.Equal(2, snapshot.Navigation.Count);
        Assert.Equal("documents.all", snapshot.Navigation.First().Key);
        Assert.DoesNotContain(snapshot.Navigation, item => item.ModuleId == analytics);
    }

    [Fact]
    public async Task Access_guard_returns_enabled_decision()
    {
        var moduleId = new ModuleId("workslip.documents");
        var guard = new ModuleAccessGuard(
            new FakeEntitlementResolver(new Dictionary<ModuleId, ModuleAvailabilityState>
            {
                [moduleId] = ModuleAvailabilityState.Enabled
            }));

        var decision = await guard.RequireEnabledAsync(Query(moduleId));

        Assert.True(decision.IsEnabled);
    }

    [Fact]
    public async Task Access_guard_throws_and_preserves_denied_decision()
    {
        var moduleId = new ModuleId("workslip.analytics");
        var guard = new ModuleAccessGuard(
            new FakeEntitlementResolver(new Dictionary<ModuleId, ModuleAvailabilityState>
            {
                [moduleId] = ModuleAvailabilityState.BlockedCapability
            }));

        var exception = await Assert.ThrowsAsync<ModuleAccessDeniedException>(async () =>
            await guard.RequireEnabledAsync(Query(moduleId)));

        Assert.Equal(ModuleAvailabilityState.BlockedCapability, exception.Decision.State);
        Assert.Equal(moduleId, exception.Decision.ModuleId);
    }

    [Fact]
    public async Task Composer_rejects_duplicate_navigation_keys()
    {
        var first = new ModuleId("module.first");
        var second = new ModuleId("module.second");
        var host = new ModuleHostDefinition(
            ApplicationId,
            HostContractVersion,
            [
                new ModuleHostRegistration(
                    first,
                    [new ModuleNavigationDescriptor("shared", "First", "/first")]),
                new ModuleHostRegistration(
                    second,
                    [new ModuleNavigationDescriptor("shared", "Second", "/second")])
            ]);

        var composer = new ModuleHostComposer(new FakeEntitlementResolver(new Dictionary<ModuleId, ModuleAvailabilityState>()));

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await composer.ComposeAsync(TenantId, host));
    }

    private static ModuleEntitlementQuery Query(ModuleId moduleId) =>
        new(TenantId, ApplicationId, moduleId, HostContractVersion);

    private sealed class FakeEntitlementResolver : IModuleEntitlementResolver
    {
        private readonly IReadOnlyDictionary<ModuleId, ModuleAvailabilityState> _states;

        public FakeEntitlementResolver(IReadOnlyDictionary<ModuleId, ModuleAvailabilityState> states)
        {
            _states = states;
        }

        public ValueTask<ModuleEntitlementDecision> ResolveAsync(
            ModuleEntitlementQuery query,
            CancellationToken cancellationToken = default)
        {
            var state = _states.TryGetValue(query.ModuleId, out var configured)
                ? configured
                : ModuleAvailabilityState.UnknownModule;

            var decision = new ModuleEntitlementDecision(
                query.TenantId,
                query.ApplicationId,
                query.ModuleId,
                state,
                new ModuleVersion("1.0.0"),
                new ModuleContractVersion("1.0.0"),
                Array.Empty<ModuleDependencyFailure>(),
                Array.Empty<CapabilityDecision>(),
                Reason: "test decision");

            return ValueTask.FromResult(decision);
        }
    }
}
