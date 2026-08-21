namespace MR.SAASy.Contracts.Motor.Deployment;

public sealed record DeploymentGateDecision(
    bool IsReadyToDeploy,
    DeploymentStage Stage,
    IReadOnlyCollection<string> Reasons);
