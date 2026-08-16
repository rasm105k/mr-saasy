namespace MR.SAASy.Contracts.Identity;

/// <summary>
/// Stable platform-owned identity identifier. Product user IDs and provider subjects are external references.
/// </summary>
public readonly record struct IdentityId(string Value)
{
    public override string ToString() => Value;
}
