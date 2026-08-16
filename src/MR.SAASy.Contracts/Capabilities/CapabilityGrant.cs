using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Tenant;

namespace MR.SAASy.Contracts.Capabilities;

public sealed record CapabilityGrant(
    TenantId TenantId,
    ApplicationId ApplicationId,
    CapabilityKey CapabilityKey,
    bool Enabled,
    CapabilityGrantSource Source);
