namespace MR.SAASy.Contracts.Motor.Mcp;

/// <summary>Connector targets prepared by MOTOR-001; these constants do not perform I/O.</summary>
public static class KnownMcpConnectors
{
    public static readonly McpConnectorKey Azure = new("azure");
    public static readonly McpConnectorKey GitHub = new("github");
    public static readonly McpConnectorKey Linear = new("linear");
    public static readonly McpConnectorKey Workslip = new("workslip");
    public static readonly McpConnectorKey OpenCodeZen = new("opencode-zen");
}
