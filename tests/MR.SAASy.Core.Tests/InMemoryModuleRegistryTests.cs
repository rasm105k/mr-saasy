using MR.SAASy.Contracts.Modules;
using MR.SAASy.Core.Modules;
using Xunit;

namespace MR.SAASy.Core.Tests;

public sealed class InMemoryModuleRegistryTests
{
    private static readonly ModuleId Support = new("workslip.support");

    [Fact]
    public async Task Finds_a_seeded_module()
    {
        var registry = new InMemoryModuleRegistry([Manifest(Support, "1.0.0")]);

        var manifest = await registry.FindAsync(Support);

        Assert.NotNull(manifest);
        Assert.Equal(Support, manifest.ModuleId);
    }

    [Fact]
    public async Task Version_mismatch_returns_no_manifest()
    {
        var registry = new InMemoryModuleRegistry([Manifest(Support, "1.0.0")]);

        Assert.Null(await registry.FindAsync(Support, new ModuleVersion("2.0.0")));
    }

    [Fact]
    public async Task Lists_all_seeded_modules()
    {
        var registry = new InMemoryModuleRegistry(
        [
            Manifest(Support, "1.0.0"),
            Manifest(new ModuleId("platform.audit"), "1.1.0"),
        ]);

        var modules = await registry.ListAsync();

        Assert.Equal(2, modules.Count);
    }

    private static ModuleManifest Manifest(ModuleId id, string version) =>
        new(
            id,
            id.Value,
            new ModuleVersion(version),
            new ModuleContractVersion("1.0.0"),
            Array.Empty<ModuleDependency>(),
            Array.Empty<RequiredCapability>(),
            Array.Empty<ProvidedCapability>(),
            new ModuleCompatibility(new ModuleContractVersion("1.0.0"), new ModuleContractVersion("1.9.9")));
}
