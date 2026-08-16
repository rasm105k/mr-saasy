using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Capabilities;
using MR.SAASy.Contracts.Modules;
using MR.SAASy.Contracts.Modules.Entitlements;
using MR.SAASy.Contracts.Tenant;

namespace MR.SAASy.Core.Modules;

/// <summary>
/// Provider-neutral, fail-closed module availability resolver.
/// It combines tenant/application binding, module manifests, dependency health,
/// host compatibility, and capability decisions without reading product data directly.
/// </summary>
public sealed class ModuleEntitlementResolver : IModuleEntitlementResolver
{
    private readonly ITenantDirectory _tenantDirectory;
    private readonly IModuleRegistry _moduleRegistry;
    private readonly ICapabilityRegistry _capabilityRegistry;

    public ModuleEntitlementResolver(
        ITenantDirectory tenantDirectory,
        IModuleRegistry moduleRegistry,
        ICapabilityRegistry capabilityRegistry)
    {
        _tenantDirectory = tenantDirectory ?? throw new ArgumentNullException(nameof(tenantDirectory));
        _moduleRegistry = moduleRegistry ?? throw new ArgumentNullException(nameof(moduleRegistry));
        _capabilityRegistry = capabilityRegistry ?? throw new ArgumentNullException(nameof(capabilityRegistry));
    }

    public async ValueTask<ModuleEntitlementDecision> ResolveAsync(
        ModuleEntitlementQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var tenant = await _tenantDirectory.FindAsync(query.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Disabled(query, "Tenant is not registered in MR SAAS'y.");
        }

        if (tenant.LifecycleState != TenantLifecycleState.Active)
        {
            return Disabled(query, $"Tenant lifecycle is {tenant.LifecycleState}; Active is required.");
        }

        var bindings = await _tenantDirectory.GetApplicationBindingsAsync(query.TenantId, cancellationToken);
        var activeBinding = bindings.FirstOrDefault(binding =>
            binding.ApplicationId == query.ApplicationId &&
            binding.State == TenantApplicationBindingState.Active);

        if (activeBinding is null)
        {
            return Disabled(query, "Tenant does not have an active binding to the requested application.");
        }

        var manifest = await _moduleRegistry.FindAsync(
            query.ModuleId,
            query.RequestedVersion,
            cancellationToken);

        if (manifest is null)
        {
            return new ModuleEntitlementDecision(
                query.TenantId,
                query.ApplicationId,
                query.ModuleId,
                ModuleAvailabilityState.UnknownModule,
                ResolvedVersion: null,
                ResolvedContractVersion: null,
                DependencyFailures: Array.Empty<ModuleDependencyFailure>(),
                CapabilityDecisions: Array.Empty<CapabilityDecision>(),
                Reason: "Module is not registered.");
        }

        return await ResolveManifestAsync(
            query,
            manifest,
            new HashSet<ModuleId>(),
            cancellationToken);
    }

    private async ValueTask<ModuleEntitlementDecision> ResolveManifestAsync(
        ModuleEntitlementQuery query,
        ModuleManifest manifest,
        HashSet<ModuleId> resolutionPath,
        CancellationToken cancellationToken)
    {
        if (!resolutionPath.Add(manifest.ModuleId))
        {
            return Decision(
                query,
                manifest,
                ModuleAvailabilityState.BlockedDependency,
                reason: $"Circular required dependency detected at module '{manifest.ModuleId.Value}'.");
        }

        try
        {
            if (!IsHostCompatible(query.HostContractVersion, manifest.Compatibility, out var compatibilityReason))
            {
                return Decision(
                    query,
                    manifest,
                    ModuleAvailabilityState.UnsupportedVersion,
                    reason: compatibilityReason);
            }

            var capabilityDecisions = new List<CapabilityDecision>(manifest.RequiredCapabilities.Count);
            foreach (var requiredCapability in manifest.RequiredCapabilities)
            {
                var capabilityDecision = await _capabilityRegistry.ResolveAsync(
                    query.TenantId,
                    query.ApplicationId,
                    requiredCapability.CapabilityKey,
                    cancellationToken);

                capabilityDecisions.Add(capabilityDecision);
            }

            var blockedCapability = capabilityDecisions.FirstOrDefault(decision => !decision.IsEnabled);
            if (blockedCapability is not null)
            {
                return Decision(
                    query,
                    manifest,
                    ModuleAvailabilityState.BlockedCapability,
                    capabilityDecisions: capabilityDecisions,
                    reason: $"Required capability '{blockedCapability.CapabilityKey.Value}' resolved as {blockedCapability.State}.");
            }

            var dependencyFailures = new List<ModuleDependencyFailure>();
            foreach (var dependency in manifest.Dependencies.Where(dependency => !dependency.Optional))
            {
                var dependencyManifest = await _moduleRegistry.FindAsync(
                    dependency.ModuleId,
                    version: null,
                    cancellationToken);

                if (dependencyManifest is null)
                {
                    dependencyFailures.Add(new ModuleDependencyFailure(
                        dependency.ModuleId,
                        ModuleAvailabilityState.UnknownModule,
                        dependency.MinimumVersion,
                        ResolvedVersion: null,
                        "Required dependency is not registered."));
                    continue;
                }

                if (!MeetsMinimumVersion(
                        dependencyManifest.ImplementationVersion,
                        dependency.MinimumVersion,
                        out var versionReason))
                {
                    dependencyFailures.Add(new ModuleDependencyFailure(
                        dependency.ModuleId,
                        ModuleAvailabilityState.UnsupportedVersion,
                        dependency.MinimumVersion,
                        dependencyManifest.ImplementationVersion,
                        versionReason));
                    continue;
                }

                var dependencyDecision = await ResolveManifestAsync(
                    query with
                    {
                        ModuleId = dependency.ModuleId,
                        RequestedVersion = dependencyManifest.ImplementationVersion
                    },
                    dependencyManifest,
                    resolutionPath,
                    cancellationToken);

                if (!dependencyDecision.IsEnabled)
                {
                    dependencyFailures.Add(new ModuleDependencyFailure(
                        dependency.ModuleId,
                        dependencyDecision.State,
                        dependency.MinimumVersion,
                        dependencyDecision.ResolvedVersion,
                        dependencyDecision.Reason ?? "Required dependency is unavailable."));
                }
            }

            if (dependencyFailures.Count > 0)
            {
                return Decision(
                    query,
                    manifest,
                    ModuleAvailabilityState.BlockedDependency,
                    dependencyFailures,
                    capabilityDecisions,
                    "One or more required module dependencies are unavailable.");
            }

            return Decision(
                query,
                manifest,
                ModuleAvailabilityState.Enabled,
                capabilityDecisions: capabilityDecisions,
                reason: "Module requirements are satisfied.");
        }
        finally
        {
            resolutionPath.Remove(manifest.ModuleId);
        }
    }

    private static bool IsHostCompatible(
        ModuleContractVersion hostVersion,
        ModuleCompatibility compatibility,
        out string reason)
    {
        if (!SemanticVersionComparer.TryCompare(
                hostVersion.Value,
                compatibility.MinimumHostContractVersion.Value,
                out var minimumComparison) ||
            !SemanticVersionComparer.TryCompare(
                hostVersion.Value,
                compatibility.MaximumHostContractVersion.Value,
                out var maximumComparison))
        {
            reason = "Host/module contract versions must use semantic version format (major.minor.patch).";
            return false;
        }

        if (minimumComparison < 0 || maximumComparison > 0)
        {
            reason = $"Host contract {hostVersion.Value} is outside supported range " +
                     $"{compatibility.MinimumHostContractVersion.Value}..{compatibility.MaximumHostContractVersion.Value}.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static bool MeetsMinimumVersion(
        ModuleVersion resolvedVersion,
        ModuleVersion minimumVersion,
        out string reason)
    {
        if (!SemanticVersionComparer.TryCompare(
                resolvedVersion.Value,
                minimumVersion.Value,
                out var comparison))
        {
            reason = "Module implementation versions must use semantic version format (major.minor.patch).";
            return false;
        }

        if (comparison < 0)
        {
            reason = $"Resolved version {resolvedVersion.Value} is below required minimum {minimumVersion.Value}.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static ModuleEntitlementDecision Disabled(
        ModuleEntitlementQuery query,
        string reason) =>
        new(
            query.TenantId,
            query.ApplicationId,
            query.ModuleId,
            ModuleAvailabilityState.Disabled,
            ResolvedVersion: null,
            ResolvedContractVersion: null,
            DependencyFailures: Array.Empty<ModuleDependencyFailure>(),
            CapabilityDecisions: Array.Empty<CapabilityDecision>(),
            Reason: reason);

    private static ModuleEntitlementDecision Decision(
        ModuleEntitlementQuery query,
        ModuleManifest manifest,
        ModuleAvailabilityState state,
        IReadOnlyCollection<ModuleDependencyFailure>? dependencyFailures = null,
        IReadOnlyCollection<CapabilityDecision>? capabilityDecisions = null,
        string? reason = null) =>
        new(
            query.TenantId,
            query.ApplicationId,
            manifest.ModuleId,
            state,
            manifest.ImplementationVersion,
            manifest.ContractVersion,
            dependencyFailures ?? Array.Empty<ModuleDependencyFailure>(),
            capabilityDecisions ?? Array.Empty<CapabilityDecision>(),
            reason);
}
