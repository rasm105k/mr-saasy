using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Capabilities;
using MR.SAASy.Contracts.Modules;
using MR.SAASy.Contracts.Modules.Entitlements;
using MR.SAASy.Contracts.Tenant;
using MR.SAASy.Core.Modules;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class ModuleEntitlementResolverTests
{
    private static readonly TenantId TenantId = new("tenant-a");
    private static readonly ApplicationIdentifier ApplicationId = new("workslip");
    private static readonly ModuleContractVersion HostContractVersion = new("1.2.0");

    [Fact]
    public async Task Enables_module_when_all_requirements_are_satisfied()
    {
        var dependency = Manifest("platform.audit", "1.1.0");
        var root = Manifest(
            "workslip.documents",
            dependencies: [new ModuleDependency(dependency.ModuleId, new ModuleVersion("1.0.0"))],
            requiredCapabilities: [new RequiredCapability(new CapabilityKey("workslip.documents"))]);

        var resolver = Resolver(
            modules: [root, dependency],
            capabilityStates: new Dictionary<CapabilityKey, CapabilityDecisionState>
            {
                [new CapabilityKey("workslip.documents")] = CapabilityDecisionState.Enabled
            });

        var decision = await resolver.ResolveAsync(Query(root.ModuleId));

        Assert.True(decision.IsEnabled);
        Assert.Equal(ModuleAvailabilityState.Enabled, decision.State);
        Assert.Empty(decision.DependencyFailures);
        Assert.Single(decision.CapabilityDecisions);
    }

    [Fact]
    public async Task Blocks_when_required_capability_is_not_enabled()
    {
        var root = Manifest(
            "workslip.documents",
            requiredCapabilities: [new RequiredCapability(new CapabilityKey("workslip.documents"))]);

        var resolver = Resolver(
            modules: [root],
            capabilityStates: new Dictionary<CapabilityKey, CapabilityDecisionState>
            {
                [new CapabilityKey("workslip.documents")] = CapabilityDecisionState.Disabled
            });

        var decision = await resolver.ResolveAsync(Query(root.ModuleId));

        Assert.False(decision.IsEnabled);
        Assert.Equal(ModuleAvailabilityState.BlockedCapability, decision.State);
        Assert.Single(decision.CapabilityDecisions);
    }

    [Fact]
    public async Task Blocks_when_required_dependency_is_missing()
    {
        var root = Manifest(
            "workslip.documents",
            dependencies: [new ModuleDependency(new ModuleId("platform.audit"), new ModuleVersion("1.0.0"))]);

        var resolver = Resolver(modules: [root]);
        var decision = await resolver.ResolveAsync(Query(root.ModuleId));

        Assert.Equal(ModuleAvailabilityState.BlockedDependency, decision.State);
        var failure = Assert.Single(decision.DependencyFailures);
        Assert.Equal(ModuleAvailabilityState.UnknownModule, failure.State);
    }

    [Fact]
    public async Task Blocks_when_dependency_version_is_below_minimum()
    {
        var dependency = Manifest("platform.audit", "0.9.0");
        var root = Manifest(
            "workslip.documents",
            dependencies: [new ModuleDependency(dependency.ModuleId, new ModuleVersion("1.0.0"))]);

        var resolver = Resolver(modules: [root, dependency]);
        var decision = await resolver.ResolveAsync(Query(root.ModuleId));

        Assert.Equal(ModuleAvailabilityState.BlockedDependency, decision.State);
        var failure = Assert.Single(decision.DependencyFailures);
        Assert.Equal(ModuleAvailabilityState.UnsupportedVersion, failure.State);
        Assert.Equal("0.9.0", failure.ResolvedVersion?.Value);
    }

    [Fact]
    public async Task Rejects_module_outside_host_contract_range()
    {
        var root = Manifest(
            "workslip.documents",
            minimumHostContract: "2.0.0",
            maximumHostContract: "2.9.9");

        var resolver = Resolver(modules: [root]);
        var decision = await resolver.ResolveAsync(Query(root.ModuleId));

        Assert.Equal(ModuleAvailabilityState.UnsupportedVersion, decision.State);
    }

    [Fact]
    public async Task Inactive_application_binding_fails_closed()
    {
        var root = Manifest("workslip.documents");
        var resolver = Resolver(
            modules: [root],
            bindingState: TenantApplicationBindingState.Suspended);

        var decision = await resolver.ResolveAsync(Query(root.ModuleId));

        Assert.Equal(ModuleAvailabilityState.Disabled, decision.State);
    }

    [Fact]
    public async Task Missing_optional_dependency_does_not_block_module()
    {
        var root = Manifest(
            "workslip.documents",
            dependencies:
            [
                new ModuleDependency(
                    new ModuleId("platform.optional-analytics"),
                    new ModuleVersion("1.0.0"),
                    Optional: true)
            ]);

        var resolver = Resolver(modules: [root]);
        var decision = await resolver.ResolveAsync(Query(root.ModuleId));

        Assert.True(decision.IsEnabled);
    }

    [Fact]
    public async Task Circular_required_dependencies_fail_closed()
    {
        var firstId = new ModuleId("module.first");
        var secondId = new ModuleId("module.second");
        var first = Manifest(
            firstId.Value,
            dependencies: [new ModuleDependency(secondId, new ModuleVersion("1.0.0"))]);
        var second = Manifest(
            secondId.Value,
            dependencies: [new ModuleDependency(firstId, new ModuleVersion("1.0.0"))]);

        var resolver = Resolver(modules: [first, second]);
        var decision = await resolver.ResolveAsync(Query(first.ModuleId));

        Assert.Equal(ModuleAvailabilityState.BlockedDependency, decision.State);
        Assert.NotEmpty(decision.DependencyFailures);
    }

    private static ModuleEntitlementQuery Query(ModuleId moduleId) =>
        new(TenantId, ApplicationId, moduleId, HostContractVersion);

    private static ModuleManifest Manifest(
        string id,
        string implementationVersion = "1.0.0",
        IReadOnlyCollection<ModuleDependency>? dependencies = null,
        IReadOnlyCollection<RequiredCapability>? requiredCapabilities = null,
        string minimumHostContract = "1.0.0",
        string maximumHostContract = "1.9.9") =>
        new(
            new ModuleId(id),
            id,
            new ModuleVersion(implementationVersion),
            new ModuleContractVersion("1.0.0"),
            dependencies ?? Array.Empty<ModuleDependency>(),
            requiredCapabilities ?? Array.Empty<RequiredCapability>(),
            Array.Empty<ProvidedCapability>(),
            new ModuleCompatibility(
                new ModuleContractVersion(minimumHostContract),
                new ModuleContractVersion(maximumHostContract)));

    private static ModuleEntitlementResolver Resolver(
        IReadOnlyCollection<ModuleManifest> modules,
        IReadOnlyDictionary<CapabilityKey, CapabilityDecisionState>? capabilityStates = null,
        TenantLifecycleState tenantState = TenantLifecycleState.Active,
        TenantApplicationBindingState bindingState = TenantApplicationBindingState.Active) =>
        new(
            new FakeTenantDirectory(tenantState, bindingState),
            new FakeModuleRegistry(modules),
            new FakeCapabilityRegistry(capabilityStates));

    private sealed class FakeTenantDirectory : ITenantDirectory
    {
        private readonly TenantDescriptor _tenant;
        private readonly IReadOnlyCollection<TenantApplicationBinding> _bindings;

        public FakeTenantDirectory(
            TenantLifecycleState tenantState,
            TenantApplicationBindingState bindingState)
        {
            _tenant = new TenantDescriptor(TenantId, "Test Tenant", tenantState);
            _bindings =
            [
                new TenantApplicationBinding(
                    TenantId,
                    ApplicationId,
                    new ExternalTenantReference("workslip.organization", "org-test"),
                    bindingState)
            ];
        }

        public ValueTask<TenantDescriptor?> FindAsync(
            TenantId tenantId,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult<TenantDescriptor?>(tenantId == TenantId ? _tenant : null);

        public ValueTask<IReadOnlyCollection<TenantApplicationBinding>> GetApplicationBindingsAsync(
            TenantId tenantId,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(
                tenantId == TenantId
                    ? _bindings
                    : (IReadOnlyCollection<TenantApplicationBinding>)Array.Empty<TenantApplicationBinding>());
    }

    private sealed class FakeModuleRegistry : IModuleRegistry
    {
        private readonly IReadOnlyDictionary<ModuleId, ModuleManifest> _modules;

        public FakeModuleRegistry(IEnumerable<ModuleManifest> modules)
        {
            _modules = modules.ToDictionary(module => module.ModuleId);
        }

        public ValueTask<ModuleManifest?> FindAsync(
            ModuleId moduleId,
            ModuleVersion? version = null,
            CancellationToken cancellationToken = default)
        {
            if (!_modules.TryGetValue(moduleId, out var manifest))
            {
                return ValueTask.FromResult<ModuleManifest?>(null);
            }

            if (version is not null && manifest.ImplementationVersion != version.Value)
            {
                return ValueTask.FromResult<ModuleManifest?>(null);
            }

            return ValueTask.FromResult<ModuleManifest?>(manifest);
        }

        public ValueTask<IReadOnlyCollection<ModuleManifest>> ListAsync(
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult<IReadOnlyCollection<ModuleManifest>>(_modules.Values.ToArray());
    }

    private sealed class FakeCapabilityRegistry : ICapabilityRegistry
    {
        private readonly IReadOnlyDictionary<CapabilityKey, CapabilityDecisionState> _states;

        public FakeCapabilityRegistry(
            IReadOnlyDictionary<CapabilityKey, CapabilityDecisionState>? states)
        {
            _states = states ?? new Dictionary<CapabilityKey, CapabilityDecisionState>();
        }

        public ValueTask<CapabilityDescriptor?> FindAsync(
            CapabilityKey capabilityKey,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult<CapabilityDescriptor?>(null);

        public ValueTask<CapabilityDecision> ResolveAsync(
            TenantId tenantId,
            ApplicationIdentifier applicationId,
            CapabilityKey capabilityKey,
            CancellationToken cancellationToken = default)
        {
            var state = _states.TryGetValue(capabilityKey, out var configuredState)
                ? configuredState
                : CapabilityDecisionState.Unknown;

            return ValueTask.FromResult(
                new CapabilityDecision(
                    tenantId,
                    applicationId,
                    capabilityKey,
                    state,
                    CapabilityGrantSource.SystemPolicy));
        }
    }
}
