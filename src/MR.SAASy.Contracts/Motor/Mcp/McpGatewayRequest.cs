using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Mcp;

/// <summary>
/// Gateway request. Argument values are passed to the connector but must never be copied
/// into MOTOR events, decisions or memory.
/// </summary>
public sealed record McpGatewayRequest(
    ToolCall ToolCall,
    MotorProjectContext Project,
    IReadOnlyDictionary<string, string?> Arguments,
    Approval? Approval = null);
