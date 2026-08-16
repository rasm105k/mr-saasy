# Module Entitlement Resolver

## Purpose

The Module Entitlement Resolver is the first executable decision layer in MR SAAS'y's module model.

It answers:

> May this tenant use this module for this application on this host contract version?

The resolver is provider-neutral and fail-closed. It does not read Workslip or another product database directly.

## Inputs

A decision is scoped by:

- `TenantId`
- `ApplicationIdentifier`
- `ModuleId`
- host `ModuleContractVersion`
- optional requested module implementation version

## Decision order

1. Tenant exists and is `Active`.
2. Tenant has an `Active` binding to the application.
3. Module manifest exists.
4. Host contract version is compatible with the module manifest.
5. Every required capability resolves to `Enabled`.
6. Every required module dependency exists.
7. Every required dependency satisfies its minimum implementation version.
8. Required dependencies recursively resolve as enabled.
9. Circular required dependencies fail closed.

Optional dependencies never block the base module. Optional functionality must still be explicitly guarded before it is exposed.

## Availability states

- `Enabled`
- `Disabled`
- `BlockedDependency`
- `BlockedCapability`
- `UnsupportedVersion`
- `UnknownModule`

Only `Enabled` authorizes module availability.

## Runtime boundary

The implementation lives in `MR.SAASy.Core` and depends only on public platform contracts:

```text
ModuleEntitlementResolver
   ├─ ITenantDirectory
   ├─ IModuleRegistry
   └─ ICapabilityRegistry
```

No database implementation, cloud provider SDK, product model, frontend state, or runtime plugin loader is part of this slice.

## Version semantics

Compatibility and dependency minimums use semantic version precedence (`major.minor.patch`, including pre-release precedence). Invalid version metadata fails closed rather than falling back to string comparison.

## Authorization boundary

Module availability is platform authorization for whether a module may exist for a tenant/application composition. It does not replace product-domain authorization for user actions or resources.

Frontend navigation may reflect a resolver decision, but backend enforcement remains authoritative.
