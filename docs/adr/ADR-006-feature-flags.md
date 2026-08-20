# ADR-006 Feature flags live in the platform

Status: Accepted

Experimental UI and delight features are evaluated by MR SAAS'y, not by each product.

- Default off.
- Platform kill cannot be overridden by tenant, identity or application.
- Identity/tenant/application overrides exist so superusers and users can turn a feature off for themselves.
- Products consume `IFeatureFlagEvaluator`. They do not own the kill switch.
- Help copy is registered in `IHelpCatalog` so the same assistant can serve Workslip and later services.
