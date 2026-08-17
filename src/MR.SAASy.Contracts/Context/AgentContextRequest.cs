using MR.SAASy.Contracts.Access;
using MR.SAASy.Contracts.Capabilities;
using MR.SAASy.Contracts.Identity;

namespace MR.SAASy.Contracts.Context;

/// <summary>
/// A single agent-facing request to read product context: who is asking (<paramref name="IdentityId"/>),
/// where and as what (<paramref name="Scope"/>, <paramref name="Role"/>), for which
/// <paramref name="Capability"/>, and which <paramref name="RequestedFields"/>.
/// </summary>
public sealed record AgentContextRequest(
    IdentityId IdentityId,
    AccessScope Scope,
    AccessRoleKey Role,
    CapabilityKey Capability,
    IReadOnlyCollection<ContextFieldKey> RequestedFields);
