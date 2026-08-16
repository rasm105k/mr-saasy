# Module Registry boundary

## Purpose

MR SAAS'y owns module metadata and the stable contracts that describe reusable platform/product capabilities.

A module is declared through metadata. v0.1 does **not** introduce a runtime plugin loader, dynamic assembly loading, or a microservice requirement.

## Manifest model

Each module declares:

- `ModuleId`
- implementation version
- public contract version
- dependencies on other module IDs
- required capabilities
- provided capabilities
- supported host contract range

Implementation version and contract version are deliberately separate. A module may ship implementation changes without forcing a new public contract version.

## Dependency rules

Dependencies reference stable `ModuleId` values and versions, never .NET classes, assemblies, routes, provider SDK types, or product-domain types.

Circular required dependencies are forbidden. Optional dependencies may be declared but must never silently enable functionality.

## Capability relationship

The Module Registry does not duplicate entitlement logic.

```text
Module Registry
  declares required/provided CapabilityKey values
          |
          v
Capability Registry
  decides whether TenantId + ApplicationId may use them
```

Only the Capability Registry grants access. A module manifest is descriptive metadata, not authorization.

## Product boundary

Products may compose modules, but MR SAAS'y core must not import product-domain code. Workslip-specific modules can be declared later while Workslip remains owner of its domain workflows and data.

## Runtime boundary

v0.1 is compatible with multiple future hosting models:

- in-process modular monolith
- HTTP/service adapter
- separately deployed service

The contract does not force one of these models.

## v0.1 scope

The first contract slice contains:

- `ModuleId`
- `ModuleVersion`
- `ModuleContractVersion`
- `ModuleDependency`
- `RequiredCapability`
- `ProvidedCapability`
- `ModuleCompatibility`
- `ModuleManifest`
- `IModuleRegistry`

This slice adds no module persistence, tenant entitlement resolver, navigation host, runtime plugin loader, product adapter, or Workslip code change.

## Next slice

The next layer is a tenant-aware Module Entitlement Resolver that combines:

- tenant/application binding
- module manifest dependencies
- capability decisions
- compatibility state

and returns an explicit module availability decision without making frontend state authoritative.
