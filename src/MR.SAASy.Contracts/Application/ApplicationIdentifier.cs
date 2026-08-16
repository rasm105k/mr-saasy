namespace MR.SAASy.Contracts.Application;

/// <summary>
/// Stable platform-owned application identity. Product display names, repository names,
/// deployment names, and provider resource IDs must not be used as the application identity.
/// </summary>
public readonly record struct ApplicationIdentifier(string Value)
{
    public override string ToString() => Value;
}
