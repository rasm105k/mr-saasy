using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Agents;

/// <summary>Versionable agent contract containing no credentials or provider SDK types.</summary>
public sealed record AgentDefinition(
    AgentId Id,
    string Name,
    string Role,
    AgentAccessMode DefaultAccessMode,
    IReadOnlyCollection<string> Capabilities,
    IReadOnlyCollection<AgentPermission> Permissions,
    IReadOnlyCollection<AgentApprovalRequirement> ApprovalRequirements);
