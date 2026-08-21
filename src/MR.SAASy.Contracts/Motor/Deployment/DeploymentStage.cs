namespace MR.SAASy.Contracts.Motor.Deployment;

public enum DeploymentStage
{
    Plan = 0,
    Validate = 1,
    WhatIf = 2,
    CostCheck = 3,
    Approval = 4,
    ReadyToDeploy = 5,
    Deploy = 6,
    EventLog = 7,
    Blocked = 8,
}
