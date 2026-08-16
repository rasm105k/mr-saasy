namespace MR.SAASy.Contracts.Modules;

/// <summary>
/// Implementation/release version of a module. This may change without changing the public contract version.
/// </summary>
public readonly record struct ModuleVersion(string Value)
{
    public override string ToString() => Value;
}
