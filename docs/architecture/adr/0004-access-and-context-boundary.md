# ADR 0004 — Access & Context Boundary

## Status
Accepted

Implemented in `rasm105k/mr-saasy` PR #1 (WOR-574). Supersedes the earlier PHP prototype parked in `Workslip-v2.0` PR #767 (loop work belongs in the platform repo per ADR-001).

## Context
AI agents need to read product context, but the platform must decide *whether* an agent may read it and *which fields* it may see — without holding customer data or importing product authorization models (ADR-001, ADR-004, `architecture/overview.md`).

Two concerns are easy to conflate and must stay separate:

- **Authorization** — is this identity, in this scope, with this role, allowed the capability at all?
- **Context shaping** — given that it is allowed, which requested fields are exposed, and which must be masked?

Both must be fail-closed: the default answer is "no access / no field", and absence of an explicit grant is a denial, not a gap.

## Decision
Model the boundary as two provider-neutral stages composed behind one agent-facing entry point.

### 1. Access decision — `IAccessGrantResolver`
`AccessGrantResolver` combines identity lifecycle (`IIdentityDirectory`), explicit grants (`IAccessGrantStore`), scope-completeness validation and grant expiry (injected `TimeProvider`) into an `AccessGrantDecision`.

| Situation | Decision |
|---|---|
| Invalid / incomplete scope combination | `Unsupported` |
| Identity not registered | `Unknown` |
| Identity not `Active` | `Denied` |
| No matching, unexpired grant | `Denied` |
| Matching grant expired | `Denied` |
| Active identity + matching unexpired grant | `Granted` |

There is **no implicit scope or role cascade**: a grant authorizes only the exact scope and role it names. A `Platform` grant does not satisfy a `Tenant` request, and a grant for one tenant never satisfies another — tenants are isolated by default.

### 2. Context shaping — `IContextProjectionResolver`
Given a capability and the fields an agent requested, `ContextProjectionResolver` returns a `ContextProjectionPlan`: `granted = requested ∩ permitted`, `masked = granted ∩ policy.masked`, `denied = the remainder`. An unknown capability grants nothing. Permitted/masked field sets come from `IContextFieldPolicy`.

The plan names **field keys only** — never values. The product owns its data and applies the plan (return granted fields, mask the masked subset, omit denied). No customer value ever enters the platform. The plan also exposes `PlaintextFields` (granted minus masked) as the set that is safe to return as raw values, so a consumer never leaks masked fields by returning "granted" verbatim.

### 3. Composition — `IAgentContextGateway`
`AgentContextGateway` is the single boundary an agent crosses. It authorizes first; **only on a `Granted` decision** does it resolve the projection plan. A denied request yields no field plan and never invokes the projector.

## Rules
- The platform decides; the product applies. Values never cross into the platform.
- Fail closed at every stage: unknown/invalid inputs and missing grants deny.
- No implicit authorization cascade across scope or role.
- No product-domain or provider SDK imports in platform contracts or core (ADR-001, ADR-004). Product role → platform grant mapping is an explicit, reviewable adapter concern owned by the consumer.
- Grant lifecycle and identity lifecycle are separate; an active identity with zero grants has zero access.
- Each decision enum's default (0) value is a non-authorizing state, so a default-initialized decision never authorizes.
- Every gateway request and decision is emitted to an `IAuditSink` as metadata only — identifiers, field names, decision state and reason — never customer field values.

## Consequences
- The platform ships in-memory reference implementations (`InMemoryIdentityDirectory`, `InMemoryAccessGrantStore`, `CapabilityContextFieldPolicy`, `InMemoryAuditSink`) for local/dev and tests; no persistence is introduced (ADR-004).
- A concrete, product-owned adapter — mapping real product roles/identities to platform grants and supplying a real `IContextFieldPolicy` — remains a consumer-side (Workslip repo) task and is intentionally out of the platform.
- The Executive/agent layers can rely on a single, testable authorization + minimization surface rather than each agent re-deriving access rules.

## References
- Linear: WOR-574
- `architecture/access-grant-contract.md`
- ADR-001 (separate platform repository), ADR-002 (contract-first), ADR-003 (tenant-first), ADR-004 (no shared database)
