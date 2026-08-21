# Repository bridge: MR SAAS'y ↔ product repositories

## Decision

Product repositories are not linked with Git submodules, shared source folders, or direct database access.

The bridge is contract-based:

```text
mr-saasy
  └─ owns MR.SAASy.Contracts
          ↓ versioned package/release
product repository
  └─ product-owned adapter
          ↓
product application/domain
```

For Workslip, the runtime call direction is:

```text
Workslip domain/application
        ↓
Workslip-owned platform adapter
        ↓
MR.SAASy.Contracts and/or platform API
```

The compile-time dependency must never point back from MR SAAS'y core into Workslip domain, EF entities, repositories, or product-specific DTOs.

## Why

This keeps platform and products independently deployable and makes contract upgrades explicit, versioned, testable, and reversible. It also allows Workslip functionality to be strangler-migrated one bounded slice at a time instead of requiring a shared database or big-bang rewrite.

## GitHub model

1. `mr-saasy` owns and versions shared contract packages.
2. Product profiles in `mr-saasy/products/*` identify known consumers and their repositories.
3. A product chooses when to adopt a contract version.
4. The product implements its adapter in its own repository.
5. MR SAAS'y never imports product domain code or reads product databases directly.
6. Provider-specific IDs and SDK payloads are translated or reduced before crossing the adapter boundary.

## Package strategy

During foundation work, CI produces `MR.SAASy.Contracts` as a build artifact only.

When the contract is proven stable, publish versioned packages to GitHub Packages (NuGet). Products must pin an explicit compatible version; `main`-to-`main` source coupling is forbidden.

## Workslip ownership boundary

Workslip remains owner of its product domain and persistence, including organizations, filials, users, customers, jobs, KLS, worksheets and reference data. MR SAAS'y owns platform identities, registries, platform grants and shared platform contracts.

A Workslip adapter may read Workslip application/domain data through Workslip-owned ports and translate it to platform contracts. It must not expose Workslip EF entities or repository interfaces to MR SAAS'y, and MR SAAS'y must not write Workslip tables directly.

Workslip-specific identifiers are external references at the platform boundary, not platform primary identifiers.

## First Workslip adapter slices

### 1. Organization → platform tenant

Current Workslip source: `OrganizationRow.Id` is the product-owned `Guid`; `OrganizationRow` also owns product business fields and `Filials`.

Platform mapping:

```text
Workslip OrganizationRow.Id
  → ExternalTenantReference(
      ReferenceType = "workslip.organization",
      Value = canonical opaque organization id)
  → TenantApplicationBinding(application = "workslip")
  → MR SAAS'y TenantId
```

Rules:

- `TenantId` remains platform-owned and must not equal or derive semantic meaning from `OrganizationRow.Id`.
- The external reference value is stable and opaque; repeated synchronization of the same organization must resolve to the same binding.
- A Workslip filial is product substructure, not a platform tenant.
- CVR, customers, jobs, KLS, worksheets and other Workslip domain data do not belong in the tenant identity contract.
- Platform suspension/decommissioning changes platform lifecycle/binding state; it does not directly mutate Workslip organization domain state.

Current platform owner: `MR.SAASy.Contracts.Tenant` / `ITenantDirectory` and `docs/architecture/tenant-registry.md`.

Parity evidence before cutover:

- two distinct Workslip organizations map to distinct external references and tenant bindings;
- replaying the same organization is idempotent;
- a non-Workslip tenant can coexist without a Workslip organization;
- filial IDs never create tenant records.

Rollback: stop consuming the platform tenant lookup and continue using Workslip organization ownership; do not delete the legacy organization path until parity evidence is retained.

### 2. Environment/application metadata → Application Registry

Current Workslip source: repository/deployment metadata and environment configuration. The registered platform product profile uses stable `applicationId = "workslip"`.

Platform mapping:

- the stable product identity maps to `ApplicationIdentifier("workslip")`;
- Workslip runtime/deployment environments map to the platform `ApplicationEnvironment` values;
- repository names, Azure resource IDs, App Service names and Entra object IDs are metadata/external references, never the application primary identity.

Current platform owner: `MR.SAASy.Contracts.Application` / `IApplicationRegistry` plus `products/workslip/profile.json`.

Parity evidence before cutover:

- every supported Workslip environment resolves to one expected `ApplicationDescriptor`;
- provider/resource IDs cannot change the stable `ApplicationIdentifier`;
- an unknown environment fails explicitly rather than being coerced to Production.

Rollback: pin Workslip to its existing environment configuration while retaining the platform registry as non-authoritative until parity is proven.

### 3. Superadmin/platform access → Identity & Access

Current Workslip source: Workslip users, Entra subjects, product roles and existing privileged administration/bootstrap flows.

Platform mapping:

- human/service identities cross the boundary as platform identities plus external identity subjects;
- Workslip product roles remain Workslip-owned unless an explicit mapping produces a platform `AccessGrant`;
- platform grants use platform scopes and must not reuse Workslip role enums as the platform authorization model;
- superadmin/bootstrap identity is a platform concern, while ordinary in-tenant Workslip authorization stays enforced by Workslip.

Current platform owners: `MR.SAASy.Contracts.Identity`, `MR.SAASy.Contracts.Access`, `IIdentityDirectory`, `IAccessGrantResolver`, and the accepted access/context boundary.

Parity evidence before cutover:

- mapped identities retain stable external-subject linkage;
- product-only roles do not accidentally gain platform scope;
- tenant/application/environment grants remain scope-exact and fail closed;
- existing Workslip authorization remains authoritative for product requests during migration.

Rollback: disable the platform grant consumer and retain Workslip's existing product authorization path. Never remove the Workslip permission path in the same step that first enables the platform mapping.

### 4. Key Vault/App Configuration references → Secrets & Configuration contract

Current Workslip source: deployment-owned Azure Key Vault and App Configuration references.

Boundary rule:

- the adapter may expose provider-neutral secret/configuration references, ownership, scope and status;
- secret values, connection strings, tokens and provider SDK payloads never cross into platform core;
- Azure resource identifiers remain adapter/provider metadata rather than shared domain identities.

The target contract is owned by the Secrets & Configuration platform work (WOR-506). Until that contract exists, this slice must not invent a Workslip-specific parallel DTO inside MR SAAS'y.

Parity evidence before cutover:

- reference identity/scope matches the existing Workslip configuration source;
- no secret material appears in serialized contract fixtures, logs or tests;
- missing/inaccessible references remain explicit failure/unknown states rather than fabricated values.

Rollback: keep Workslip's current Key Vault/App Configuration resolution path authoritative and disable only the projection/adapter.

### 5. Health/diagnostics/deployment state → Operations contract

Current Workslip source includes the public `/health` endpoint, Workslip diagnostics and deployment/monitoring evidence. `/health` currently returns an explicit `status = "ok"` response; richer diagnostics remain product/provider data.

Boundary rule:

- adapters normalize product/provider observations into platform health/operations summaries;
- raw Application Insights logs, exception payloads, SQL details and diagnostics objects do not become platform-core contracts;
- observation timestamp/freshness and evidence references stay explicit;
- unknown, blocked, degraded and unhealthy conditions must not be collapsed into healthy.

The target contract is owned by Operations & Observability work (WOR-507 and its contract slices). Do not create a second Workslip-only health contract to bridge the gap.

Parity evidence before cutover:

- the current healthy Workslip signal maps to the expected normalized state;
- stale/missing evidence remains distinguishable from healthy;
- evidence references point back to the owning source without copying sensitive payloads.

Rollback: stop publishing/consuming the normalized projection; Workslip monitoring and health endpoints continue unchanged.

### 6. Workslip delivery profile → Product Delivery Contract

Current platform source: `products/workslip/profile.json` identifies Workslip, its repository and the planned `contract-package-and-product-adapter` integration mode.

Boundary rule:

- repository, branch, environment, release and deployment evidence must integrate through the Product Delivery Contract owned by WOR-498/WOR-501;
- this bridge does not define a competing delivery model;
- Workslip remains independently deployable and chooses when to adopt a compatible platform contract version.

Parity evidence before cutover:

- the platform delivery projection identifies the same Workslip repository/application/environment as the existing delivery configuration;
- exact revision/evidence references remain traceable to the product source of truth;
- a second product can use the same contract without Workslip-specific fields.

Rollback: retain the existing Workslip delivery pipeline/profile and disable the platform consumer; no deployment path is removed until multi-product parity is demonstrated.

## Migration sequence for every slice

Each adapter slice follows the same reversible sequence:

1. Define or adopt the provider-neutral platform contract.
2. Add a Workslip-owned adapter around the current Workslip application/domain owner.
3. Add contract/parity fixtures at the adapter boundary.
4. Introduce the MR SAAS'y implementation/consumer behind the contract.
5. Run both paths long enough to compare identity, authorization or read-model semantics where dual-read/shadow comparison is safe.
6. Switch the consumer only after the required evidence is green.
7. Remove legacy ownership only in a later change after rollback is no longer required.

Dual-write of Workslip product state is not the migration mechanism. If a slice requires a mutation, it must use an explicit authorized command boundary with idempotency/concurrency semantics owned by that slice.

## Cross-boundary test contract

Every implementation slice must prove the risk of that boundary rather than only testing DTO serialization:

- identifier translation is deterministic and collision-safe;
- tenant/application scope is preserved;
- authorization fails closed and does not widen product roles;
- retries/replays are idempotent where synchronization occurs;
- provider failures do not create partial product writes;
- provider-specific or sensitive payloads do not leak into platform contracts;
- the legacy product path remains usable until the replacement path has parity evidence.

## Forbidden dependencies

```text
MR SAAS'y Core → Workslip.Domain
MR SAAS'y Core → Workslip EF entities/repositories
MR SAAS'y Core → Workslip database
Shared platform module → direct Workslip database write
Workslip feature → unversioned mr-saasy source checkout
Platform contract → Workslip-specific customer/job/KLS/worksheet types
```

## Workslip status

Workslip is the first planned consumer. Its product profile may live in this repository, but runtime Workslip integration is performed only through explicit Workslip-owned adapter slices. This document defines the boundary; it does not itself migrate product data, remove Workslip APIs/UI, or require a microservice split.
