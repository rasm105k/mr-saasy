using MR.SAASy.Contracts.Application;
using Xunit;

namespace MR.SAASy.Contracts.Tests;

public sealed class ApplicationDescriptorTests
{
    [Fact]
    public void Descriptor_is_product_neutral_and_stable()
    {
        var descriptor = new ApplicationDescriptor(
            new ApplicationId("workslip"),
            "Workslip",
            "0.0.0-test",
            ApplicationEnvironment.Test);

        Assert.Equal("workslip", descriptor.ApplicationId.Value);
        Assert.Equal(ApplicationEnvironment.Test, descriptor.Environment);
    }

    [Fact]
    public void Same_contract_can_describe_a_second_product()
    {
        var descriptor = new ApplicationDescriptor(
            new ApplicationId("synthetic-product"),
            "Synthetic Product",
            "1.0.0",
            ApplicationEnvironment.Development);

        Assert.DoesNotContain("Workslip", descriptor.GetType().FullName ?? string.Empty);
        Assert.Equal("synthetic-product", descriptor.ApplicationId.Value);
    }
}
