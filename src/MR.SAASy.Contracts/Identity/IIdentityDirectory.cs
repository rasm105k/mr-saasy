namespace MR.SAASy.Contracts.Identity;

public interface IIdentityDirectory
{
    ValueTask<IdentityDescriptor?> FindAsync(
        IdentityId identityId,
        CancellationToken cancellationToken = default);

    ValueTask<IdentityDescriptor?> FindByExternalSubjectAsync(
        ExternalIdentitySubject subject,
        CancellationToken cancellationToken = default);
}
