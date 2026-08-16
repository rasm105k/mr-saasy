namespace MR.SAASy.Contracts.Identity;

public sealed record IdentityDescriptor(
    IdentityId IdentityId,
    IdentityKind Kind,
    string DisplayName,
    IdentityLifecycleState LifecycleState,
    IReadOnlyCollection<ExternalIdentitySubject> ExternalSubjects,
    string? Email = null);
