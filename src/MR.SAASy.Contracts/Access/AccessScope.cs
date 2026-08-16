using MR.SAASy.Contracts.Application;
using MR.SAASy.Contracts.Tenant;

namespace MR.SAASy.Contracts.Access;

/// <summary>
/// Explicit access scope. Consumers must validate the fields required by the selected scope kind and fail closed on invalid combinations.
/// </summary>
public sealed record AccessScope(
    AccessScopeKind Kind,
    ApplicationIdentifier? ApplicationId = null,
    TenantId? TenantId = null,
    ApplicationEnvironment? Environment = null);
