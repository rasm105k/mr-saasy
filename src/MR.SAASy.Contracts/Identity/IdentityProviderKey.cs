namespace MR.SAASy.Contracts.Identity;

/// <summary>
/// Stable provider/directory key such as entra or a product-owned external identity directory.
/// </summary>
public readonly record struct IdentityProviderKey(string Value)
{
    public override string ToString() => Value;
}
