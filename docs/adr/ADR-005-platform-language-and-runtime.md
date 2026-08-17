# ADR-005: C# is the platform language; the durable runtime stays polyglot behind contracts

## Status
Accepted

## Decision

C#/.NET is the default language for the MR SAAS'y platform, and the `mr-saasy` repository is the platform of record.

- New platform logic — contracts, policy, orchestration, registries — is written in C#/.NET in `mr-saasy`.
- The PHP/Laravel control plane (Workslip repo, `platform/mr-saasy-control-plane`) is **superseded**. Its domain — role routing, executive hierarchy, and the access/context boundary — has been re-homed to C# (WOR-574, ADR 0004). It is not extended further; any remaining unique behaviour is ported to C# rather than added to Laravel.
- The Python durable-execution runtime (Workslip repo, `platform/mr-saasy-agent-poc` — a Temporal workflow plus a Docker sandbox broker) is **retained as a polyglot runtime behind provider-neutral contracts**. It is not rewritten. Moving it to Temporal's .NET SDK is an explicit, separately-triggered future decision, not implied here.

## Reasoning

Everything else in the estate is C#. Standardizing the platform on one language reduces cognitive load and maintenance cost, and removes the "two half-platforms" drift risk of maintaining parallel control planes.

Laravel's batteries-included strengths served the prototype well, but a contract-first .NET library gains little from a web framework, and the control plane's domain has already moved to C#.

The durable-loop runtime is a different case. Temporal's durable execution and container-based sandbox isolation are exactly the out-of-the-box value worth keeping, and it is the one piece that actually runs today. Rewriting it now would be cost without product benefit. A language boundary is acceptable when it sits behind a provider-neutral contract.

## Consequence

- Stop extending the Laravel control plane; treat it as superseded and port any still-needed behaviour to `mr-saasy`.
- Keep the Python runtime reachable only through provider-neutral contracts; no product-domain or platform-internal coupling may leak across the boundary.
- All new platform work targets C#/.NET in `mr-saasy`.
- Retiring the Workslip-side `platform/` prototype folders and adding a superseded pointer in their READMEs is a follow-up in the Workslip repo, done when convenient (kept separate for now).
- A future C#/.NET reimplementation of the durable runtime stays open and must be its own ADR if pursued.

## References

- ADR-001 (separate platform repository), ADR-002 (contract-first)
- `architecture/adr/0004-access-and-context-boundary.md`, Linear WOR-574
- Superseded/retained prototype: `Workslip-v2.0` `platform/mr-saasy-control-plane` (PHP, superseded) and `platform/mr-saasy-agent-poc` (Python runtime, retained behind contracts)
