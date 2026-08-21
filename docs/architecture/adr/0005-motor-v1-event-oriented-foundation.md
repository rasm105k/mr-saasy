# ADR 0005 — MOTOR v1 is a bounded, event-oriented orchestration module

## Status

Accepted for MOTOR-001.

## Context

MR SAAS'y needs a safe orchestration engine for specialist agents, model routing,
external tools, memory and deployment workflows. Adding provider SDKs or autonomous
write paths directly to platform core would violate the existing contract-first and
access-boundary decisions.

## Decision

MOTOR is introduced as a bounded module inside the existing platform projects:

- provider-neutral public contracts live in `MR.SAASy.Contracts/Motor`;
- policies and in-memory reference implementations live in `MR.SAASy.Core/Motor`;
- external systems are reachable only through `IMcpGateway` and `IMcpConnector`;
- permissions are explicit per agent, connector, operation and level; absence denies;
- destructive or declared governed actions require a current human approval bound to
  the exact mission, project and action reference; deployment approvals additionally bind
  the exact What-If evidence reference;
- model names and current prices are runtime candidates, while logical routes remain stable;
- important actions use versioned typed events plus a flattened JSON envelope;
- memory records decisions, solutions, agent/model performance, business impact and
  human feedback without storing prompts, secrets or customer payload values;
- infrastructure stays Bicep and all MOTOR modules are disabled by default;
- deployment execution is out of scope; a pure gate proves readiness only after plan,
  validation, What-If, cost evidence and bound approval.

The routing precedence is deliberately quality-first:

1. high/critical risk uses premium reasoning;
2. unknown task or risk uses premium reasoning and requires evaluation;
3. code targets the logical OpenCode ZEN route;
4. explicitly low-value or bulk work uses a suitable economy route;
5. high complexity uses premium reasoning;
6. all other work defaults to premium reasoning and evaluation.

Historical quality and success influence selection within a route. Cost breaks ties for
quality-first routes and leads selection within economy routes.

## Consequences

- MOTOR can be tested without Azure, MCP providers or model credentials.
- EventStoreDB, MassTransit, Polly, Data Lake and Power BI remain future adapters behind
  the new contracts; MOTOR-001 does not add persistence or transport dependencies.
- Azure, GitHub, Linear, Workslip and OpenCode ZEN are connector targets only. There is
  no live invocation in this milestone.
- In-memory stores are local/test reference implementations, not production durability.
- No agent receives unlimited access and no deployment adapter exists yet.

## References

- ADR-001 separate platform repository
- ADR-002 contract-first
- ADR-003 tenant-first
- ADR-004 no shared database
- ADR-005 platform language and runtime
- ADR-006 Bicep infrastructure as code
