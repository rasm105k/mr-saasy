# Capability Registry boundary

## Purpose

MR SAAS'y owns platform capability metadata and tenant/application entitlement decisions.

A capability answers a narrow question:

> May this tenant use this platform/product capability for this application?

It does not replace product-domain authorization. A product may still deny an action based on its own roles, workflow state, resource ownership, or business rules.

## Scope

Every decision is scoped by:

- `TenantId`
- `ApplicationId`
- `CapabilityKey`

Example capability keys should be stable and namespaced, such as `workslip.time-tracking` or `platform.audit`.

## Fail-closed semantics

Decision states are explicit:

- `Enabled`
- `Disabled`
- `Unknown`
- `Unsupported`

Only `Enabled` grants capability access.

`Unknown` and `Unsupported` must never be treated as enabled. This prevents missing configuration, stale provider data, or unsupported capability versions from silently granting access.

## Grant sources

The contract can identify where a decision originated:

- plan
- subscription
- admin override
- system policy

Provider-specific billing or feature-flag types must stay behind adapters.

## Product boundary

The Capability Registry may decide that a tenant has access to a capability, but product authorization remains authoritative for domain actions.

Example:

```text
MR SAAS'y: tenant has workslip.documents = Enabled
                 |
                 v
Workslip: may this user edit this specific case document? -> product authorization
```

## v0.1 scope

The first contract slice contains:

- `CapabilityKey`
- `CapabilityDescriptor`
- `CapabilityGrant`
- `CapabilityGrantSource`
- `CapabilityDecision`
- `CapabilityDecisionState`
- `ICapabilityRegistry`

This slice is contract-only. It introduces no persistence, billing provider, feature flag provider, UI, or Workslip code change.
