using MR.SAASy.Contracts.Application;

namespace MR.SAASy.Contracts.Tenant;

public sealed record TenantApplicationBinding(
    TenantId TenantId,
    ApplicationIdentifier ApplicationId,
    ExternalTenantReference? ExternalTenantReference,
    TenantApplicationBindingState State);
