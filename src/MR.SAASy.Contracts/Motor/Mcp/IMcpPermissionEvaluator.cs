namespace MR.SAASy.Contracts.Motor.Mcp;

public interface IMcpPermissionEvaluator
{
    ValueTask<McpPermissionDecision> EvaluateAsync(
        McpGatewayRequest request,
        CancellationToken cancellationToken = default);
}
