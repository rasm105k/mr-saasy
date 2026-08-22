namespace MR.SAASy.Core.Motor.Reconciliation;

/// <summary>
/// First MOTOR reconciliation boundary.
/// Keeps observation, comparison and action separate so future controllers can
/// evolve without coupling to providers or execution engines.
/// </summary>
public sealed class MissionReconciler
{
    public ReconciliationResult Reconcile(
        DesiredState desired,
        IReadOnlyDictionary<string, string> actual)
    {
        var differences = desired.Requirements
            .Where(x => !actual.TryGetValue(x.Key, out var value) || value != x.Value)
            .ToDictionary(x => x.Key, x => x.Value);

        return differences.Count == 0
            ? ReconciliationResult.Converged(desired.MissionId)
            : ReconciliationResult.DriftDetected(desired.MissionId, differences);
    }
}

public sealed record ReconciliationResult(
    string MissionId,
    bool IsConverged,
    IReadOnlyDictionary<string, string> Drift)
{
    public static ReconciliationResult Converged(string id) =>
        new(id, true, new Dictionary<string, string>());

    public static ReconciliationResult DriftDetected(
        string id,
        IReadOnlyDictionary<string, string> drift) =>
        new(id, false, drift);
}
