using MR.SAASy.Contracts.Identity;

namespace MR.SAASy.Contracts.Access;

/// <summary>
/// Provider-neutral source of the explicit access grants held by an identity.
/// Implementations must not import product authorization models or provider SDK types;
/// grants are mapped into platform contract values by the owning adapter.
/// </summary>
public interface IAccessGrantStore
{
    ValueTask<IReadOnlyCollection<AccessGrant>> GetGrantsAsync(
        IdentityId identityId,
        CancellationToken cancellationToken = default);
}
