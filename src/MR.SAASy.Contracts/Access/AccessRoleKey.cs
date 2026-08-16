namespace MR.SAASy.Contracts.Access;

/// <summary>
/// Stable namespaced platform role key such as platform.superadmin or application.operator.
/// Product role enums are mapped explicitly by adapters and never imported here.
/// </summary>
public readonly record struct AccessRoleKey(string Value)
{
    public override string ToString() => Value;
}
