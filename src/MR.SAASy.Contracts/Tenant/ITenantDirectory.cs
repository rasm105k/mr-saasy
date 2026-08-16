namespace MR.SAASy.Contracts.Tenant;

public interface ITenantDirectory
{
    ValueTask<TenantDescriptor?> FindAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyCollection<TenantApplicationBinding>> GetApplicationBindingsAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default);
}
