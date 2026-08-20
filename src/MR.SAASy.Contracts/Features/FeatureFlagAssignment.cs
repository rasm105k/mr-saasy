using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Identity;
using MR.SAASy.Contracts.Tenant;

namespace MR.SAASy.Contracts.Features;

/// <summary>
/// One seeded decision. Platform kill uses only Flag.
/// Application/tenant/identity scopes fill the matching identifiers.
/// </summary>
public sealed record FeatureFlagAssignment(
    FeatureFlagKey Flag,
    FeatureFlagState State,
    FeatureFlagSource Source,
    ApplicationIdentifier? ApplicationId = null,
    TenantId? TenantId = null,
    IdentityId? IdentityId = null);
