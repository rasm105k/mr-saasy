namespace MR.SAASy.Contracts.Motor.Deployment;

public interface ISafeDeploymentGate
{
    DeploymentGateDecision Evaluate(
        SafeDeploymentRequest request,
        DeploymentReadinessEvidence evidence);
}
