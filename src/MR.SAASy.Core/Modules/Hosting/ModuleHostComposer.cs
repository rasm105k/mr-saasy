using MR.SAASy.Contracts.Modules.Entitlements;
using MR.SAASy.Contracts.Modules.Hosting;
using MR.SAASy.Contracts.Tenant;

namespace MR.SAASy.Core.Modules.Hosting;

/// <summary>
/// Projects a product host definition into tenant-specific enabled modules and navigation.
/// Navigation is UX metadata only; backend operations must use IModuleAccessGuard.
/// </summary>
public sealed class ModuleHostComposer : IModuleHostComposer
{
    private readonly IModuleEntitlementResolver _resolver;

    public ModuleHostComposer(IModuleEntitlementResolver resolver)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }

    public async ValueTask<ModuleHostSnapshot> ComposeAsync(
        TenantId tenantId,
        ModuleHostDefinition hostDefinition,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(hostDefinition);
        ValidateDefinition(hostDefinition);

        var enabledModules = new List<ModuleId>();
        var navigation = new List<ModuleNavigationEntry>();

        foreach (var registration in hostDefinition.Modules)
        {
            var decision = await _resolver.ResolveAsync(
                new ModuleEntitlementQuery(
                    tenantId,
                    hostDefinition.ApplicationId,
                    registration.ModuleId,
                    hostDefinition.HostContractVersion),
                cancellationToken);

            if (!decision.IsEnabled)
            {
                continue;
            }

            enabledModules.Add(registration.ModuleId);
            navigation.AddRange(
                registration.Navigation.Select(item =>
                    new ModuleNavigationEntry(
                        registration.ModuleId,
                        item.Key,
                        item.Label,
                        item.Route,
                        item.Order)));
        }

        var orderedNavigation = navigation
            .OrderBy(item => item.Order)
            .ThenBy(item => item.Key, StringComparer.Ordinal)
            .ToArray();

        return new ModuleHostSnapshot(
            tenantId,
            hostDefinition.ApplicationId,
            enabledModules.ToArray(),
            orderedNavigation);
    }

    private static void ValidateDefinition(ModuleHostDefinition hostDefinition)
    {
        ArgumentNullException.ThrowIfNull(hostDefinition.Modules);

        var duplicateModule = hostDefinition.Modules
            .GroupBy(registration => registration.ModuleId)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateModule is not null)
        {
            throw new ArgumentException(
                $"Module '{duplicateModule.Key}' is registered more than once in the host definition.",
                nameof(hostDefinition));
        }

        var navigation = hostDefinition.Modules
            .SelectMany(registration =>
            {
                ArgumentNullException.ThrowIfNull(registration.Navigation);
                return registration.Navigation;
            })
            .ToArray();

        if (navigation.Any(item =>
                string.IsNullOrWhiteSpace(item.Key) ||
                string.IsNullOrWhiteSpace(item.Label) ||
                string.IsNullOrWhiteSpace(item.Route)))
        {
            throw new ArgumentException(
                "Navigation descriptors require non-empty key, label, and route values.",
                nameof(hostDefinition));
        }

        var duplicateNavigationKey = navigation
            .GroupBy(item => item.Key, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateNavigationKey is not null)
        {
            throw new ArgumentException(
                $"Navigation key '{duplicateNavigationKey.Key}' is registered more than once in the host definition.",
                nameof(hostDefinition));
        }
    }
}
