using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Contracts.Motor.Deployment;

public sealed record SafeDeploymentRequest(
    MissionId MissionId,
    MotorProjectContext Project,
    string TargetEnvironment,
    string TemplatePath,
    string ActionReference,
    RiskLevel Risk,
    decimal MaximumApprovedMonthlyCost);
