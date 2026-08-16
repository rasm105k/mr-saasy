namespace MR.SAASy.Contracts.Tenant;

public sealed record TenantApplicationBinding(
    TenantId TenantId,
    string ApplicationId,
    ExternalTenantReference? ExternalTenantReference,
    TenantApplicationBindingState State);
