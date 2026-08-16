using MR.SAASy.Contracts.Tenant;

namespace MR.SAASy.Contracts.Capabilities;

public sealed record CapabilityGrant(
    TenantId TenantId,
    string ApplicationId,
    CapabilityKey CapabilityKey,
    bool Enabled,
    CapabilityGrantSource Source);
