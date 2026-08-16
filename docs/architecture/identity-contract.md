# Identity Contract

## Purpose

MR SAAS'y owns a stable platform identity that is independent of product user tables and provider-specific identifiers.

The contract distinguishes three first-class identity kinds:

- `Human`
- `Service`
- `Automation`

Service and automation identities are not represented as fake human users.

## Platform identity

`IdentityId` is owned by MR SAAS'y and remains stable across provider or product metadata changes.

An `IdentityDescriptor` contains:

- platform `IdentityId`
- identity kind
- display name
- explicit lifecycle state
- zero or more external subjects
- optional email metadata

Email is mutable metadata and is never the platform identity key.

## External subjects

`ExternalIdentitySubject` combines an `IdentityProviderKey` with an opaque provider-owned subject ID.

Examples include:

- an Entra object ID
- a GitHub/OIDC subject
- a product user identifier during migration

MR SAAS'y does not reinterpret or share the provider's persistence model.

## Provider boundary

The public contract contains no Microsoft Graph, Entra SDK, GitHub SDK, Keeper SDK, or product-domain types.

Provider-specific lookup/provisioning implementations belong behind adapters.

## Lifecycle

Identity lifecycle is explicit:

- `Provisioning`
- `Active`
- `Suspended`
- `Decommissioned`

Lifecycle is distinct from access grants. An active identity has no implicit permissions.

## Directory port

`IIdentityDirectory` supports lookup by platform ID or external subject. It does not grant access and it does not provision identities.

Access grants and provisioning are separate contracts so identity existence never implies authorization.
