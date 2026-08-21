namespace MR.SAASy.Contracts.Motor.Domain;

/// <summary>
/// Names the workspace, project, customer and environment a mission belongs to without
/// importing any product-domain type. Metadata must contain identifiers only.
/// </summary>
public sealed record MotorProjectContext(
    string WorkspaceId,
    string ProjectId,
    string ProjectName,
    string Environment,
    string? CustomerId = null,
    IReadOnlyDictionary<string, string>? Metadata = null);
