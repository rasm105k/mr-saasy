using MR.SAASy.Contracts.Tenant;

namespace MR.SAASy.Core.Tenant;

/// <summary>
/// Provider-neutral, in-memory <see cref="ITenantDirectory"/> seeded from explicit tenants and
/// application bindings. A platform default for local/dev and integration tests; it holds no
/// product data and performs no external lookups.
/// </summary>
public sealed class InMemoryTenantDirectory : ITenantDirectory
{
    private readonly IReadOnlyDictionary<TenantId, TenantDescriptor> _tenants;
    private readonly IReadOnlyDictionary<TenantId, IReadOnlyCollection<TenantApplicationBinding>> _bindings;

    public InMemoryTenantDirectory(
        IEnumerable<TenantDescriptor> tenants,
        IEnumerable<TenantApplicationBinding> bindings)
    {
        ArgumentNullException.ThrowIfNull(tenants);
        ArgumentNullException.ThrowIfNull(bindings);

        var tenantsById = new Dictionary<TenantId, TenantDescriptor>();
        foreach (var tenant in tenants)
        {
            tenantsById[tenant.TenantId] = tenant;
        }

        _tenants = tenantsById;

        var bindingsByTenant = new Dictionary<TenantId, List<TenantApplicationBinding>>();
        foreach (var binding in bindings)
        {
            if (!bindingsByTenant.TryGetValue(binding.TenantId, out var list))
            {
                list = [];
                bindingsByTenant[binding.TenantId] = list;
            }

            list.Add(binding);
        }

        _bindings = bindingsByTenant.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyCollection<TenantApplicationBinding>)pair.Value);
    }

    public ValueTask<TenantDescriptor?> FindAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(_tenants.TryGetValue(tenantId, out var tenant) ? tenant : null);

    public ValueTask<IReadOnlyCollection<TenantApplicationBinding>> GetApplicationBindingsAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(
            _bindings.TryGetValue(tenantId, out var bindings)
                ? bindings
                : Array.Empty<TenantApplicationBinding>());
}
