using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Identity;
using MR.SAASy.Contracts.Tenant;

namespace MR.SAASy.Contracts.Features;

public sealed record FeatureFlagQuery(
    FeatureFlagKey Flag,
    ApplicationIdentifier ApplicationId,
    TenantId? TenantId = null,
    IdentityId? IdentityId = null);
