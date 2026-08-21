# MR SAAS'y

MR SAAS'y is the internal enterprise control plane for MR Software products.

## Purpose

The platform provides shared capabilities for MR Software SaaS products:

- application and tenant registry
- identity and access
- module capabilities and entitlements
- feature flags and kill switches
- shared help assistant catalog
- secrets and configuration governance
- audit and compliance
- operations and release evidence
- shared platform modules such as files, notifications, analytics, integrations and billing

## Principles

- Platform before product
- Contracts before extraction
- Tenant-first architecture
- No shared product/platform database
- Products consume versioned platform contracts
- Provider-specific SDK types stay behind adapters
- No product-domain imports in platform core

## Repository bridge

Products are connected through versioned contracts and product-owned adapters, not Git submodules or shared source folders.

`mr-saasy` owns `MR.SAASy.Contracts`. During foundation work CI builds and packs the contract package as an artifact. Once stable, releases can publish the package to GitHub Packages and consumers can pin explicit versions.

Workslip is registered as the first planned consumer in `products/workslip/profile.json`. This does not modify the Workslip repository.

## Current layout

```text
src/
  MR.SAASy.Contracts/          # stable provider-neutral contracts
  MR.SAASy.Core/               # orchestration/policy + in-memory reference implementations
  MR.SAASy.ApplicationRegistry/# placeholder (no code yet; the reference impl lives in MR.SAASy.Core)

tests/
  MR.SAASy.Contracts.Tests/
  MR.SAASy.Core.Tests/

products/
  workslip/profile.json        # planned consumer metadata only

docs/
  architecture/                # overview + numbered architecture ADRs (0002–0004)
  adr/                         # platform ADRs (ADR-001–005)

.github/workflows/
  build.yml                    # builds + tests both test projects (csproj-driven; no .sln)
```

## Foundation
## Feature flags

`IFeatureFlagEvaluator` is the shared kill switch for experimental features across products.

- Default off.
- Platform kill beats tenant, identity and application.
- First shared flag: `platform.help-wizard`.
- Help copy lives in `IHelpCatalog` so Workslip and later services can reuse the same assistant.

## Foundation v0.1

Implemented so far — contract-first in `MR.SAASy.Contracts`, with provider-neutral in-memory reference implementations and tests in `MR.SAASy.Core` / `MR.SAASy.Core.Tests`, and no persistence:

- application, tenant, capability and module registries (contracts + `InMemory*` reference implementations);
- the access & context boundary (ADR 0004): `AccessGrantResolver`, context projection (minimization + masking), the `AgentContextGateway`, and audit emission;
- an end-to-end composition test wiring the whole platform from reference parts.

No database, Azure dependency, Keeper dependency, product adapter or Workslip code is part of this foundation.

## Build

```bash
dotnet test tests/MR.SAASy.Contracts.Tests/MR.SAASy.Contracts.Tests.csproj -c Release
dotnet test tests/MR.SAASy.Core.Tests/MR.SAASy.Core.Tests.csproj -c Release
dotnet pack src/MR.SAASy.Contracts/MR.SAASy.Contracts.csproj -c Release
```
