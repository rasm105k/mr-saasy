namespace MR.SAASy.Contracts.Tenant;

/// <summary>
/// Stable platform-owned tenant identity. Product-specific organization/customer IDs
/// must never be used as the MR SAAS'y tenant primary identity.
/// </summary>
public readonly record struct TenantId(string Value)
{
    public override string ToString() => Value;
}
