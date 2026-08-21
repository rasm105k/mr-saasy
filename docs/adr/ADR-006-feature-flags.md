# ADR-006 Feature flags are experiment/ops only

Status: Accepted

Do not duplicate capabilities.

- Capabilities = durable entitlements (tenant + application).
- Feature flags = short-lived experiment/ops with identity override and a platform kill.
- Products call `IFeatureFlagEvaluator`, then `IHelpCatalog`. No third wrapper.
- Default off. Kill cannot be overridden.
