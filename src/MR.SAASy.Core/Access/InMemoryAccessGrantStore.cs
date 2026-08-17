using MR.SAASy.Contracts.Access;
using MR.SAASy.Contracts.Identity;

namespace MR.SAASy.Core.Access;

/// <summary>
/// Provider-neutral, in-memory <see cref="IAccessGrantStore"/> seeded from an explicit set of
/// grants, indexed by identity. Intended as a platform default for local/dev and integration
/// tests: it imports no product-authorization model, performs no external lookups, and never
/// synthesizes grants that were not explicitly supplied.
/// </summary>
public sealed class InMemoryAccessGrantStore : IAccessGrantStore
{
    private readonly IReadOnlyDictionary<IdentityId, IReadOnlyCollection<AccessGrant>> _byIdentity;

    public InMemoryAccessGrantStore(IEnumerable<AccessGrant> grants)
    {
        ArgumentNullException.ThrowIfNull(grants);

        var byIdentity = new Dictionary<IdentityId, List<AccessGrant>>();
        foreach (var grant in grants)
        {
            if (!byIdentity.TryGetValue(grant.IdentityId, out var list))
            {
                list = [];
                byIdentity[grant.IdentityId] = list;
            }

            list.Add(grant);
        }

        _byIdentity = byIdentity.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyCollection<AccessGrant>)pair.Value);
    }

    public ValueTask<IReadOnlyCollection<AccessGrant>> GetGrantsAsync(
        IdentityId identityId,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(
            _byIdentity.TryGetValue(identityId, out var grants)
                ? grants
                : Array.Empty<AccessGrant>());
}
