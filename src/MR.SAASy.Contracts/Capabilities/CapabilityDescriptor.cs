namespace MR.SAASy.Contracts.Capabilities;

public sealed record CapabilityDescriptor(
    CapabilityKey Key,
    string DisplayName,
    string? Description = null);
