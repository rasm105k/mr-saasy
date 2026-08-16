namespace MR.SAASy.Contracts.Tenant;

public sealed record TenantDescriptor(
    TenantId TenantId,
    string DisplayName,
    TenantLifecycleState LifecycleState);
