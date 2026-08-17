using MR.SAASy.Contracts.Access;
using MR.SAASy.Contracts.Audit;
using MR.SAASy.Contracts.Context;

namespace MR.SAASy.Core.Context;

/// <summary>
/// Composes the access resolver and the context projection resolver into the single
/// fail-closed boundary described by WOR-574. Access is decided first; the field projection
/// plan is resolved only when the decision is <see cref="AccessGrantDecisionState.Granted"/>,
/// so a denied request never yields a field plan.
/// </summary>
/// <remarks>
/// Every request and decision is emitted to an <see cref="IAuditSink"/> as metadata only —
/// identifiers, field names, decision state and reason — never customer field values (the
/// gateway never holds values in the first place).
/// </remarks>
public sealed class AgentContextGateway : IAgentContextGateway
{
    private readonly IAccessGrantResolver _accessResolver;
    private readonly IContextProjectionResolver _projectionResolver;
    private readonly IAuditSink _auditSink;

    public AgentContextGateway(
        IAccessGrantResolver accessResolver,
        IContextProjectionResolver projectionResolver,
        IAuditSink auditSink)
    {
        _accessResolver = accessResolver ?? throw new ArgumentNullException(nameof(accessResolver));
        _projectionResolver = projectionResolver ?? throw new ArgumentNullException(nameof(projectionResolver));
        _auditSink = auditSink ?? throw new ArgumentNullException(nameof(auditSink));
    }

    public async ValueTask<AgentContextGrant> AuthorizeAsync(
        AgentContextRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _auditSink.RecordAsync(
            new AuditEvent("context.request", RequestMetadata(request)),
            cancellationToken);

        var decision = await _accessResolver.ResolveAsync(
            request.IdentityId,
            request.Scope,
            request.Role,
            cancellationToken);

        if (!decision.IsGranted)
        {
            await _auditSink.RecordAsync(
                new AuditEvent("context.decision", DecisionMetadata(request, decision, projection: null)),
                cancellationToken);

            return new AgentContextGrant(decision, Projection: null);
        }

        var projection = _projectionResolver.Resolve(request.Capability, request.RequestedFields);

        await _auditSink.RecordAsync(
            new AuditEvent("context.decision", DecisionMetadata(request, decision, projection)),
            cancellationToken);

        return new AgentContextGrant(decision, projection);
    }

    private static IReadOnlyDictionary<string, string?> RequestMetadata(AgentContextRequest request) =>
        new Dictionary<string, string?>
        {
            ["identity"] = request.IdentityId.Value,
            ["scope_kind"] = request.Scope.Kind.ToString(),
            ["application"] = request.Scope.ApplicationId?.Value,
            ["tenant"] = request.Scope.TenantId?.Value,
            ["environment"] = request.Scope.Environment?.ToString(),
            ["role"] = request.Role.Value,
            ["capability"] = request.Capability.Value,
            ["requested_fields"] = JoinFields(request.RequestedFields),
            ["requested_field_count"] = request.RequestedFields.Count.ToString(),
        };

    private static IReadOnlyDictionary<string, string?> DecisionMetadata(
        AgentContextRequest request,
        AccessGrantDecision decision,
        ContextProjectionPlan? projection) =>
        new Dictionary<string, string?>
        {
            ["capability"] = request.Capability.Value,
            ["decision"] = decision.State.ToString(),
            ["reason"] = decision.Reason,
            ["granted_fields"] = projection is null ? null : JoinFields(projection.GrantedFields),
            ["masked_fields"] = projection is null ? null : JoinFields(projection.MaskedFields),
            ["denied_fields"] = projection is null ? null : JoinFields(projection.DeniedFields),
        };

    private static string JoinFields(IReadOnlyCollection<ContextFieldKey> fields) =>
        string.Join(",", fields.Select(field => field.Value));
}
