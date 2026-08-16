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

## Why

This keeps platform and products independently deployable and makes contract upgrades explicit, versioned, testable, and reversible.

## GitHub model

1. `mr-saasy` owns and versions shared contract packages.
2. Product profiles in `mr-saasy/products/*` identify known consumers and their repositories.
3. A product chooses when to adopt a contract version.
4. The product implements its adapter in its own repository.
5. MR SAAS'y never imports product domain code or reads product databases directly.

## Package strategy

During foundation work, CI produces `MR.SAASy.Contracts` as a build artifact only.

When the contract is proven stable, publish versioned packages to GitHub Packages (NuGet). Products must pin an explicit compatible version; `main`-to-`main` source coupling is forbidden.

## Workslip

Workslip is the first planned consumer. Its profile may live in this repository, but Workslip code remains untouched until an explicit integration slice is approved.
