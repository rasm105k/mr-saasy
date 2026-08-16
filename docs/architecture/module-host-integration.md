# Module Host Integration

## Purpose

Module Host Integration turns module entitlement decisions into a safe product-host boundary.

It has two separate responsibilities:

1. **Backend enforcement** through `IModuleAccessGuard`.
2. **UX projection** through `IModuleHostComposer`.

These responsibilities intentionally share the same entitlement resolver but do not share authority.

## Host definition

A product host declares:

- `ApplicationIdentifier`
- host `ModuleContractVersion`
- the modules that product can compose
- optional navigation descriptors for each module

A host definition is product composition metadata. It is not a tenant entitlement and it is not authorization.

## Tenant projection

`ModuleHostComposer` evaluates each declared module through `IModuleEntitlementResolver` and returns only modules whose decision is `Enabled`.

Navigation entries are emitted only for enabled modules and sorted deterministically by `Order` and then `Key`.

Duplicate module registrations, duplicate navigation keys, or incomplete navigation metadata are rejected as invalid host configuration.

## Backend authority

Backend operations must use `IModuleAccessGuard.RequireEnabledAsync` before executing module-level behavior.

If the resolver returns anything other than `Enabled`, the guard throws `ModuleAccessDeniedException` and preserves the underlying entitlement decision for controlled handling/logging.

Hiding a navigation item is never sufficient authorization.

## Product-domain authorization

MR SAAS'y module availability answers:

> May this tenant/application composition use this module?

It does **not** answer:

> May this human/service identity read or change this specific product resource?

Product-domain permissions, ownership checks, workflow-state validation, and tenant-data access remain owned by the product or its controlled policy gateway.

## Framework boundary

v0.1 contains no ASP.NET middleware, MVC filters, React components, route loader, or runtime plugin loader.

The contracts can later be adapted to:

- ASP.NET endpoint filters or middleware
- server-rendered navigation
- SPA navigation/bootstrap payloads
- another host framework

without moving authorization authority into the frontend.

## Flow

```text
Product host definition
        |
        v
ModuleHostComposer --------------------> enabled navigation metadata
        |
        v
IModuleEntitlementResolver
        ^
        |
ModuleAccessGuard ---------------------> backend operation allowed/denied
```

Both paths depend on the same fail-closed entitlement source.
