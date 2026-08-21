namespace MR.SAASy.Contracts.Motor.Mcp;

/// <summary>Provider-neutral identity for an MCP connector.</summary>
public readonly record struct McpConnectorKey(string Value)
{
    public override string ToString() => Value;
}
