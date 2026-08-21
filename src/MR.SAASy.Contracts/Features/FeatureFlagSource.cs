namespace MR.SAASy.Contracts.Features;

public enum FeatureFlagSource
{
    Unknown = 0,
    DefaultOff = 1,
    PlatformKill = 2,
    Application = 3,
    Tenant = 4,
    Identity = 5
}
