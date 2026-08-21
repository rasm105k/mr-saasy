using MR.SAASy.Contracts.Motor.Domain;
using MR.SAASy.Contracts.Motor.Events;
using MR.SAASy.Contracts.Motor.Models;

namespace MR.SAASy.Core.Motor.Models;

/// <summary>
/// Deterministic MOTOR-001 routing policy. Risk and unknowns choose quality first;
/// code targets OpenCode ZEN; explicitly low-value or bulk work chooses economy.
/// </summary>
public sealed class PolicyModelRouter : IModelRouter
{
    private readonly IModelSelectionLog _selectionLog;
    private readonly IMotorEventSink _eventSink;
    private readonly TimeProvider _timeProvider;

    public PolicyModelRouter(
        IModelSelectionLog selectionLog,
        IMotorEventSink eventSink,
        TimeProvider timeProvider)
    {
        _selectionLog = selectionLog ?? throw new ArgumentNullException(nameof(selectionLog));
        _eventSink = eventSink ?? throw new ArgumentNullException(nameof(eventSink));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async ValueTask<ModelSelection> SelectAsync(
        ModelRouteRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Candidates);
        ArgumentNullException.ThrowIfNull(request.HistoricalPerformance);
        ValidateTokenEstimates(request);

        if (request.Candidates.Any(candidate =>
                candidate.InputCostPerMillionTokens < 0m || candidate.OutputCostPerMillionTokens < 0m))
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Model prices cannot be negative.");
        }

        var enabled = request.Candidates
            .Where(candidate => candidate.Enabled && candidate.SupportedTasks.Contains(request.TaskType))
            .ToArray();
        if (enabled.Length == 0)
        {
            throw new InvalidOperationException(
                "MOTOR cannot route without an enabled model candidate that explicitly supports the task type.");
        }

        var route = DetermineRoute(request);
        var candidate = SelectCandidate(request, enabled, route.Tier, route.PreferredModel);
        var evaluationRequired = route.EvaluationRequired || candidate.Tier != route.Tier;
        var reason = candidate.Tier == route.Tier
            ? route.Reason
            : $"{route.Reason} Preferred tier was unavailable; routed to {candidate.Tier} and require evaluation.";

        var selection = new ModelSelection(
            new ModelSelectionId(Guid.NewGuid().ToString("N")),
            request.MissionId,
            candidate.Key,
            candidate.Provider,
            candidate.Tier,
            reason,
            EstimateCost(request, candidate),
            evaluationRequired,
            _timeProvider.GetUtcNow());

        await _selectionLog.RecordSelectionAsync(selection, cancellationToken);
        await _eventSink.RecordAsync(
            new ModelSelected(
                new MotorEventId(Guid.NewGuid().ToString("N")),
                request.MissionId,
                request.Project,
                selection.SelectedAt,
                request.CorrelationId,
                selection.Id,
                selection.Model,
                selection.Reason,
                selection.EstimatedCost),
            cancellationToken);
        return selection;
    }

    private static (ModelTier Tier, ModelKey? PreferredModel, bool EvaluationRequired, string Reason) DetermineRoute(
        ModelRouteRequest request)
    {
        if (request.Risk is RiskLevel.High or RiskLevel.Critical)
        {
            return (ModelTier.PremiumReasoning, MotorModelKeys.PremiumReasoning, false,
                "High-risk work requires a premium reasoning route; quality takes precedence over price.");
        }

        if (request.Risk == RiskLevel.Unknown || request.TaskType == MissionTaskType.Unknown)
        {
            return (ModelTier.PremiumReasoning, MotorModelKeys.PremiumReasoning, true,
                "Unknown task or risk is routed to premium reasoning and flagged for evaluation.");
        }

        if (request.TaskType == MissionTaskType.Code)
        {
            return (ModelTier.CodeSpecialist, MotorModelKeys.OpenCodeZen, false,
                "Code work targets the OpenCode ZEN route.");
        }

        if (request.IsBulk || request.IsLowValue || request.TaskType == MissionTaskType.Bulk)
        {
            return (ModelTier.Economy, MotorModelKeys.EconomyBulk, false,
                "Explicitly low-value or bulk work uses the lowest-cost suitable route.");
        }

        if (request.Complexity == ComplexityLevel.High)
        {
            return (ModelTier.PremiumReasoning, MotorModelKeys.PremiumReasoning, false,
                "High-complexity work uses premium reasoning.");
        }

        return (ModelTier.PremiumReasoning, MotorModelKeys.PremiumReasoning, true,
            "No lower-cost rule was explicitly satisfied; quality-first premium routing is used and evaluated.");
    }

    private static ModelCandidate SelectCandidate(
        ModelRouteRequest request,
        IReadOnlyCollection<ModelCandidate> enabled,
        ModelTier targetTier,
        ModelKey? preferredModel)
    {
        var tierCandidates = enabled.Where(candidate => candidate.Tier == targetTier).ToArray();
        if (preferredModel is { } preferred)
        {
            var exact = tierCandidates.FirstOrDefault(candidate => candidate.Key == preferred);
            if (exact is not null)
            {
                return exact;
            }
        }

        if (tierCandidates.Length == 0)
        {
            tierCandidates = enabled.Where(candidate => candidate.Tier == ModelTier.PremiumReasoning).ToArray();
        }

        if (tierCandidates.Length == 0)
        {
            tierCandidates = enabled.ToArray();
        }

        var performance = request.HistoricalPerformance
            .Where(snapshot => snapshot.TaskType == request.TaskType)
            .GroupBy(snapshot => snapshot.Model)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(item => item.SampleSize).First());

        return targetTier == ModelTier.Economy
            ? tierCandidates
                .OrderBy(candidate => EstimateCost(request, candidate))
                .ThenByDescending(candidate => PerformanceScore(candidate.Key, performance))
                .ThenBy(candidate => candidate.Key.Value, StringComparer.Ordinal)
                .First()
            : tierCandidates
                .OrderByDescending(candidate => PerformanceScore(candidate.Key, performance))
                .ThenBy(candidate => EstimateCost(request, candidate))
                .ThenBy(candidate => candidate.Key.Value, StringComparer.Ordinal)
                .First();
    }

    private static decimal PerformanceScore(
        ModelKey model,
        IReadOnlyDictionary<ModelKey, ModelPerformanceSnapshot> performance) =>
        performance.TryGetValue(model, out var snapshot)
            ? (snapshot.QualityScore * 0.6m) + (snapshot.SuccessRate * 0.4m)
            : 0m;

    private static decimal EstimateCost(ModelRouteRequest request, ModelCandidate candidate) =>
        ((request.EstimatedInputTokens / 1_000_000m) * candidate.InputCostPerMillionTokens) +
        ((request.EstimatedOutputTokens / 1_000_000m) * candidate.OutputCostPerMillionTokens);

    private static void ValidateTokenEstimates(ModelRouteRequest request)
    {
        if (request.EstimatedInputTokens < 0 || request.EstimatedOutputTokens < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Estimated token counts cannot be negative.");
        }
    }
}
