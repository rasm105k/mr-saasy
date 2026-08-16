namespace MR.SAASy.Contracts.Modules;

/// <summary>
/// Version of the public module contract. Kept separate from the implementation version so compatible releases do not force consumers to upgrade contracts.
/// </summary>
public readonly record struct ModuleContractVersion(string Value)
{
    public override string ToString() => Value;
}
