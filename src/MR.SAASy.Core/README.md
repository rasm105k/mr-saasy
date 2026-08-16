# MR.SAASy.Core

Provider-neutral platform orchestration and policy implementation.

Current executable slice:

- `ModuleEntitlementResolver` — combines tenant/application binding, module manifests, capability decisions, dependency availability, and contract compatibility into a fail-closed module availability decision.

Rules:

- depend on `MR.SAASy.Contracts`, not product-domain assemblies
- no product database access
- no provider SDK types in core policy
- no secret values in durable state
- backend policy is authoritative; frontend state is descriptive only
- prefer modular-monolith boundaries before service extraction
- invalid or unknown security/entitlement state fails closed
