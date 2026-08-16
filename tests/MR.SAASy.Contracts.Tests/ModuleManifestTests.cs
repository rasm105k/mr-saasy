using MR.SAASy.Contracts.Capabilities;
using MR.SAASy.Contracts.Modules;
using Xunit;

namespace MR.SAASy.Contracts.Tests;

public sealed class ModuleManifestTests
{
    [Fact]
    public void Manifest_keeps_implementation_and_contract_versions_separate()
    {
        var manifest = CreateManifest();

        Assert.Equal("2.4.0", manifest.ImplementationVersion.Value);
        Assert.Equal("1.0", manifest.ContractVersion.Value);
    }

    [Fact]
    public void Dependencies_reference_stable_module_ids_instead_of_runtime_types()
    {
        var manifest = CreateManifest();
        var dependency = Assert.Single(manifest.Dependencies);

        Assert.Equal("platform.audit", dependency.ModuleId.Value);
        Assert.Equal("1.0.0", dependency.MinimumVersion.Value);
    }

    [Fact]
    public void Capabilities_reuse_the_shared_capability_contract()
    {
        var manifest = CreateManifest();

        Assert.Equal("platform.audit.write", Assert.Single(manifest.RequiredCapabilities).CapabilityKey.Value);
        Assert.Equal("workslip.documents", Assert.Single(manifest.ProvidedCapabilities).CapabilityKey.Value);
    }

    private static ModuleManifest CreateManifest() => new(
        new ModuleId("shared.documents"),
        "Documents",
        new ModuleVersion("2.4.0"),
        new ModuleContractVersion("1.0"),
        new[] { new ModuleDependency(new ModuleId("platform.audit"), new ModuleVersion("1.0.0")) },
        new[] { new RequiredCapability(new CapabilityKey("platform.audit.write")) },
        new[] { new ProvidedCapability(new CapabilityKey("workslip.documents")) },
        new ModuleCompatibility(new ModuleContractVersion("1.0"), new ModuleContractVersion("1.x")));
}
