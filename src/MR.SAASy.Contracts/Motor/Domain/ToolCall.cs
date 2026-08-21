using MR.SAASy.Contracts.Motor.Agents;
using MR.SAASy.Contracts.Motor.Mcp;

namespace MR.SAASy.Contracts.Motor.Domain;

/// <summary>Sanitized metadata for a requested external-tool action.</summary>
public sealed record ToolCall(
    ToolCallId Id,
    MissionId MissionId,
    AgentId AgentId,
    McpConnectorKey Connector,
    string Operation,
    PermissionLevel RequiredPermission,
    RiskLevel Risk,
    bool IsDestructive,
    ToolCallState State,
    DateTimeOffset RequestedAt,
    string CorrelationId,
    ApprovalId? ApprovalId = null);
