namespace MR.SAASy.Contracts.Context;

/// <summary>
/// Stable key naming a single field of product context an agent may request.
/// Field keys are platform contract values; product column/property names are mapped to them
/// explicitly by the owning product adapter and are never imported here.
/// </summary>
public readonly record struct ContextFieldKey(string Value)
{
    public override string ToString() => Value;
}
