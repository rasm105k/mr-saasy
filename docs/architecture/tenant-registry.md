# Tenant Registry boundary

## Purpose

MR SAAS'y owns the stable platform tenant identity and the relationship between tenants and applications.

Products remain owners of their own customer/domain records.

## Identity model

```text
MR SAAS'y TenantId
       |
       +-- TenantApplicationBinding(applicationId = "workslip")
                  |
                  +-- ExternalTenantReference("workslip.organization", "<opaque-id>")
```

The platform `TenantId` is never a Workslip `Organization.Id` or another product database key.

## Workslip mapping

When Workslip is integrated later:

- MR SAAS'y creates/owns `TenantId`.
- Workslip continues to own Organization business data and workflow semantics.
- Workslip `Organization.Id` is represented only as an opaque `ExternalTenantReference`.
- No shared database, shared EF model, or cross-repository domain import is introduced.
- A Workslip filial/branch is not automatically a platform tenant.

## Lifecycle

Tenant lifecycle is explicit:

- `Provisioning`
- `Active`
- `Suspended`
- `Decommissioned`

Application bindings have their own lifecycle because a tenant may exist independently of any one product.

## v0.1 scope

The first contract slice contains:

- `TenantId`
- `TenantDescriptor`
- `TenantLifecycleState`
- `ExternalTenantReference`
- `TenantApplicationBinding`
- `TenantApplicationBindingState`
- `ITenantDirectory`

This slice is contract-only. It adds no persistence, provisioning, product adapter, or Workslip code change.
