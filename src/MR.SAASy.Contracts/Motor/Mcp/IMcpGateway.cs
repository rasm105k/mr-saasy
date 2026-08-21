namespace MR.SAASy.Contracts.Motor.Mcp;

public interface IMcpGateway
{
    ValueTask<McpCallResult> ExecuteAsync(
        McpGatewayRequest request,
        CancellationToken cancellationToken = default);
}
