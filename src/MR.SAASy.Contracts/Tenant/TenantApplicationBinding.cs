using MR.SAASy.Contracts.Application;

namespace MR.SAASy.Contracts.Tenant;

public sealed record TenantApplicationBinding(
    TenantId TenantId,
    ApplicationId ApplicationId,
    ExternalTenantReference? ExternalTenantReference,
    TenantApplicationBindingState State);
