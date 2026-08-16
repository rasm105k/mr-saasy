using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Tenant;

namespace MR.SAASy.Contracts.Capabilities;

public interface ICapabilityRegistry
{
    ValueTask<CapabilityDescriptor?> FindAsync(
        CapabilityKey capabilityKey,
        CancellationToken cancellationToken = default);

    ValueTask<CapabilityDecision> ResolveAsync(
        TenantId tenantId,
        ApplicationIdentifier applicationId,
        CapabilityKey capabilityKey,
        CancellationToken cancellationToken = default);
}
