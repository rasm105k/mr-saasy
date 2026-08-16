namespace MR.SAASy.Contracts.Modules;

/// <summary>
/// Stable platform-owned module identifier. It is not a .NET type, assembly name, route, or deployment name.
/// </summary>
public readonly record struct ModuleId(string Value)
{
    public override string ToString() => Value;
}
