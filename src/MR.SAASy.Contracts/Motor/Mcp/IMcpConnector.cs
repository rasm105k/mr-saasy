namespace MR.SAASy.Contracts.Motor.Mcp;

/// <summary>Implemented by an external-tool adapter; platform agents never receive one directly.</summary>
public interface IMcpConnector
{
    McpConnectorKey Key { get; }

    ValueTask<McpCallResult> ExecuteAsync(
        McpGatewayRequest request,
        CancellationToken cancellationToken = default);
}
