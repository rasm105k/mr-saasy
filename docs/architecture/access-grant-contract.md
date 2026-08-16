# Access Grant Contract

## Purpose

Identity existence does not imply access. MR SAAS'y models authorization as explicit grants and fail-closed decisions.

## Core types

- `AccessGrantId`
- `AccessRoleKey`
- `AccessScope`
- `AccessGrant`
- `AccessGrantSource`
- `AccessGrantDecision`
- `IAccessGrantResolver`

## Scopes

Access can be scoped to:

- `Platform`
- `Application`
- `Tenant`
- `Environment`

Scope-specific fields must be validated by the resolver. Invalid or incomplete scope combinations must fail closed.

## Role boundary

Role keys are stable namespaced platform contract values.

Examples:

- `platform.superadmin`
- `platform.operator`
- `application.deployer`
- `workslip.admin` as an external/product role key during explicit mapping

There is no implicit rule that maps a Workslip `Admin` or any other product role to MR SAAS'y `platform.superadmin`.

Product role enums are not imported into MR SAAS'y contracts. Mapping must be explicit, reviewable, and testable in the relevant adapter/policy layer.

## Fail-closed decisions

Decision states are:

- `Granted`
- `Denied`
- `Unknown`
- `Unsupported`

Only `Granted` authorizes access.

## Grant lifecycle

A grant records its source and may have an expiry timestamp. Identity lifecycle and grant lifecycle remain separate concerns.

An active identity with zero grants has zero access by default.

## Next implementation layer

A provider-neutral access resolver will combine:

- identity lifecycle
- explicit grants
- scope validation
- expiry
- role support/policy

and produce `AccessGrantDecision` without importing product authorization models or provider SDK types.
