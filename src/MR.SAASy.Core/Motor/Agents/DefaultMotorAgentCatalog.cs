using MR.SAASy.Contracts.Motor.Agents;
using MR.SAASy.Contracts.Motor.Domain;

namespace MR.SAASy.Core.Motor.Agents;

/// <summary>The MOTOR-001 built-in specialists. No definition carries credentials.</summary>
public static class DefaultMotorAgentCatalog
{
    public static IReadOnlyCollection<AgentDefinition> Create() =>
    [
        new(
            new AgentId("gordon"),
            "Gordon",
            "Operations and infrastructure observer",
            AgentAccessMode.ReadOnly,
            ["operations", "azure", "monitoring", "infrastructure"],
            [
                Read("azure", "resources.read"),
                Read("azure", "monitoring.read"),
                Execute("azure", "deployment.what-if"),
            ],
            [Approval("azure.*.write", "Cloud mutations require an explicit human decision.")]),

        new(
            new AgentId("forge"),
            "Forge",
            "Development and pull-request specialist",
            AgentAccessMode.RestrictedWrite,
            ["development", "code-changes", "pull-requests", "opencode-zen"],
            [
                Read("github", "repository.read"),
                Write("github", "branch.write"),
                Write("github", "pull-request.create"),
                Execute("opencode-zen", "code.execute"),
                Write("github", "default-branch.write", requiresApproval: true),
                Write("github", "pull-request.merge", requiresApproval: true),
            ],
            [Approval("github.default-branch.write", "Protected branch writes require approval."),
             Approval("github.pull-request.merge", "Merge is a human-governed release action.")]),

        new(
            new AgentId("qa"),
            "QA",
            "Testing, validation and quality reviewer",
            AgentAccessMode.ReadOnly,
            ["testing", "validation", "quality-review"],
            [
                Read("github", "repository.read"),
                Execute("github", "checks.read"),
                Execute("workslip", "test.execute"),
                Write("github", "review.comment"),
            ],
            [Approval("*.production.write", "QA may recommend production changes but cannot apply them.")]),

        new(
            new AgentId("cleanup-guardian"),
            "Cleanup Guardian",
            "Cost, cleanup and documentation hygiene guardian",
            AgentAccessMode.ReadOnly,
            ["cost-control", "resource-cleanup", "documentation-hygiene"],
            [
                Read("azure", "resources.read"),
                Read("azure", "cost.read"),
                Read("github", "repository.read"),
                Write("github", "documentation.branch.write"),
                Delete("azure", "resource.delete", requiresApproval: true),
            ],
            [Approval("azure.resource.delete", "Resource deletion is destructive and always requires approval.")]),

        new(
            new AgentId("security-guardian"),
            "Security Guardian",
            "Identity, permissions and secret-governance guardian",
            AgentAccessMode.ReadOnly,
            ["security", "identity", "permissions", "secrets-governance"],
            [
                Read("azure", "identity.read"),
                Read("azure", "permissions.read"),
                Read("azure", "secret-metadata.read"),
                Write("azure", "permissions.write", requiresApproval: true),
                Write("azure", "secret.rotate", requiresApproval: true),
            ],
            [Approval("azure.permissions.write", "Role changes require a human security decision."),
             Approval("azure.secret.rotate", "Secret rotation requires an approved change window.")]),

        new(
            new AgentId("data-guardian"),
            "Data Guardian",
            "Analytics, data-quality and ML-readiness guardian",
            AgentAccessMode.ReadOnly,
            ["analytics", "data-quality", "ml-readiness"],
            [
                Read("workslip", "analytics.read"),
                Execute("workslip", "data-quality.evaluate"),
                Read("azure", "data-lake.metadata.read"),
                Write("azure", "data-lake.write", requiresApproval: true),
            ],
            [Approval("azure.data-lake.write", "Data mutation requires explicit data-owner approval.")]),
    ];

    private static AgentPermission Read(string connector, string operation) =>
        new(connector, operation, PermissionLevel.Read);

    private static AgentPermission Execute(string connector, string operation) =>
        new(connector, operation, PermissionLevel.Execute);

    private static AgentPermission Write(string connector, string operation, bool requiresApproval = false) =>
        new(connector, operation, PermissionLevel.Write, requiresApproval);

    private static AgentPermission Delete(string connector, string operation, bool requiresApproval) =>
        new(connector, operation, PermissionLevel.Delete, requiresApproval);

    private static AgentApprovalRequirement Approval(string actionPattern, string reason) =>
        new(actionPattern, RiskLevel.Low, reason);
}
