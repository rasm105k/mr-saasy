namespace MR.SAASy.Contracts.Identity;

/// <summary>
/// Opaque subject identifier owned by an external identity provider or directory.
/// </summary>
public sealed record ExternalIdentitySubject(
    IdentityProviderKey Provider,
    string SubjectId);
