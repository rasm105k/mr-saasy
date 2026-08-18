# Control Center read model boundary

## Purpose

MR SAAS'y owns a provider-neutral, read-only projection of what is running, healthy, deploying,
stale or blocked across its products. This is the platform home of the Control Center read model
(ADR 0009). The first reference implementation was built in the Workslip repository (WOR-553); these
contracts are its extraction into the platform of record (ADR-005), so the model no longer depends on
a product repository.

It answers a narrow question:

> For this application in this environment, what is the current normalized state, and where is the
> evidence?

It is not a monitoring system, a second issue tracker, or a store of provider data.

## Scope

Every projection is scoped by:

- `ApplicationIdentifier`
- `ApplicationEnvironment`

The aggregate (`ControlCenterProjection`) carries normalized health, the latest deployment evidence,
and recent automation runs for that scope.

## Fail-closed / first-class states

State is explicit and never coerced upward (ADR 0009):

- `ObservationState`: `Unknown`, `Healthy`, `Degraded`, `Unhealthy`, `Blocked`, `Stale`
- `AutomationRunState`: `Unknown`, `Queued`, `Running`, `Succeeded`, `Failed`, `Cancelled`, `Blocked`

`Unknown`, `Blocked` and `Stale` are first-class. A missing source, an unreachable provider, or an
observation older than its freshness window must never read as healthy or successful. A query for an
application with no registered source returns an explicit `Unknown` projection — never `null` and
never a healthy default — so one missing product cannot collapse unrelated Control Center state.

## Evidence, not copies

`EvidenceReference` is a provenance pointer (source, kind, opaque reference, optional URI) back to
the owning system. Raw provider payloads, logs, traces, exception bodies, secrets and customer PII do
not cross this boundary. The read model references sources of truth; it does not duplicate them.

## Provider port

`IControlCenterProjectionSource` is the product-owned adapter port. Each application implements one
source that maps its own state into these contracts. Adding a product means adding a source, not
changing the read model or its consumers (ADR 0010). Implementations must not leak product-domain
types across the boundary.

## Query boundary

`IControlCenterReadModel` is the read-only aggregation surface the Control Center BFF/UI consumes
(the contract behind WOR-597). It exposes no mutation: re-run, deploy, merge, rollback and recovery
stay in their owning systems.

## Freshness

`ControlCenterFreshnessPolicy` is a pure, clock-injected policy that reclassifies an observation as
`Stale` once it is older than a supplied maximum age. It never reinterprets `Unknown` or `Blocked`,
and it never makes a stale observation look fresh. The current time is passed in rather than read
from an ambient clock so the policy is deterministic and testable.

## Reference implementation

`MR.SAASy.Core` provides in-memory defaults for local/dev and tests:

- `StaticControlCenterProjectionSource` — a seeded, provider-neutral source (a stand-in for a real
  product adapter).
- `InMemoryControlCenterReadModel` — aggregates over registered sources by application.

No database, provider SDK, HTTP surface or product-domain import is part of this slice. The HTTP/UI
surface lands once at least one real provider adapter supplies data (WOR-597 and the Control Center
UI issues), so the platform does not ship an empty API contract.
