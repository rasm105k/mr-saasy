using MR.SAASy.Contracts.Identity;

namespace MR.SAASy.Core.Identity;

/// <summary>
/// Provider-neutral, in-memory <see cref="IIdentityDirectory"/> seeded from an explicit set
/// of identities. Intended as a platform default for local/dev and integration tests: it holds
/// no product data, performs no external lookups, and imports no provider SDK types.
/// </summary>
public sealed class InMemoryIdentityDirectory : IIdentityDirectory
{
    private readonly IReadOnlyDictionary<IdentityId, IdentityDescriptor> _byId;

    public InMemoryIdentityDirectory(IEnumerable<IdentityDescriptor> identities)
    {
        ArgumentNullException.ThrowIfNull(identities);

        var byId = new Dictionary<IdentityId, IdentityDescriptor>();
        foreach (var identity in identities)
        {
            byId[identity.IdentityId] = identity;
        }

        _byId = byId;
    }

    public ValueTask<IdentityDescriptor?> FindAsync(
        IdentityId identityId,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(_byId.TryGetValue(identityId, out var identity) ? identity : null);

    public ValueTask<IdentityDescriptor?> FindByExternalSubjectAsync(
        ExternalIdentitySubject subject,
        CancellationToken cancellationToken = default)
    {
        foreach (var identity in _byId.Values)
        {
            if (identity.ExternalSubjects.Contains(subject))
            {
                return ValueTask.FromResult<IdentityDescriptor?>(identity);
            }
        }

        return ValueTask.FromResult<IdentityDescriptor?>(null);
    }
}
