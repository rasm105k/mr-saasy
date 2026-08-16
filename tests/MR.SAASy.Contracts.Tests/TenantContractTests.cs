using MR.SAASy.Contracts.Tenant;
using Xunit;

namespace MR.SAASy.Contracts.Tests;

public sealed class TenantContractTests
{
    [Fact]
    public void Platform_tenant_identity_is_separate_from_product_identity()
    {
        var tenantId = new TenantId("ten_mr_001");
        var externalReference = new ExternalTenantReference(
            "workslip.organization",
            "7b777e38-1d5e-4a99-a745-0d6dcf2bcb13");

        var binding = new TenantApplicationBinding(
            tenantId,
            "workslip",
            externalReference,
            TenantApplicationBindingState.Planned);

        Assert.Equal("ten_mr_001", binding.TenantId.Value);
        Assert.Equal("workslip.organization", binding.ExternalTenantReference?.ReferenceType);
        Assert.NotEqual(binding.TenantId.Value, binding.ExternalTenantReference?.Value);
    }

    [Fact]
    public void Tenant_lifecycle_is_explicit_instead_of_boolean_active_flag()
    {
        var tenant = new TenantDescriptor(
            new TenantId("ten_mr_002"),
            "Example Tenant",
            TenantLifecycleState.Suspended);

        Assert.Equal(TenantLifecycleState.Suspended, tenant.LifecycleState);
        Assert.DoesNotContain(
            tenant.GetType().GetProperties(),
            property => string.Equals(property.Name, "Active", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Application_binding_can_exist_before_product_mapping_is_available()
    {
        var binding = new TenantApplicationBinding(
            new TenantId("ten_mr_003"),
            "future-product",
            ExternalTenantReference: null,
            TenantApplicationBindingState.Planned);

        Assert.Null(binding.ExternalTenantReference);
        Assert.Equal(TenantApplicationBindingState.Planned, binding.State);
    }
}
