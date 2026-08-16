using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Tenant;

namespace MR.SAASy.Contracts.Modules.Hosting;

/// <summary>
/// Tenant-specific host projection. Only enabled modules and their UX metadata are included.
/// </summary>
public sealed record ModuleHostSnapshot(
    TenantId TenantId,
    ApplicationIdentifier ApplicationId,
    IReadOnlyCollection<ModuleId> EnabledModules,
    IReadOnlyCollection<ModuleNavigationEntry> Navigation);
