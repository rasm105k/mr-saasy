using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Tenant;

namespace MR.SAASy.Contracts.Modules.Entitlements;

public sealed record ModuleEntitlementQuery(
    TenantId TenantId,
    ApplicationIdentifier ApplicationId,
    ModuleId ModuleId,
    ModuleContractVersion HostContractVersion,
    ModuleVersion? RequestedVersion = null);
