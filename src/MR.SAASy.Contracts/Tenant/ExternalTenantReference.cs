namespace MR.SAASy.Contracts.Tenant;

/// <summary>
/// Opaque reference to a tenant/customer/organization identity owned by a product.
/// MR SAAS'y may store and return this reference but must not infer product-domain meaning from it.
/// </summary>
public sealed record ExternalTenantReference(
    string ReferenceType,
    string Value);
