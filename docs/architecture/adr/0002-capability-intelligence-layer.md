# ADR 0002: Capability Intelligence Layer

## Status
Proposed

## Context
MR SAAS'y needs a shared understanding of product capabilities so navigation, AI agents, UX reviews, permissions and documentation can operate from the same source of truth.

Linear issues describe implementation work, but agents must not rely on tickets alone. They need normalized capability context.

## Decision
Introduce a Capability Intelligence Layer built around a Capability Registry.

A capability should contain:

- identity
- purpose
- category
- user roles
- permissions
- UI locations
- routes
- backend/frontend ownership
- documentation references
- audit requirements
- AI visibility rules

## Future Consumers

- User Companion Agent
- Blind UX Discoverability Agent
- Product Intelligence Agent
- Search/navigation
- Release validation

## Principles

- Capabilities must be addressable
- Capabilities must be searchable
- Capabilities must be permission-aware
- AI agents consume capability context, not raw product databases

## Roadmap

1. Capability Registry foundation
2. Context builder
3. Agent integrations
4. Anonymous action intelligence
5. Continuous UX improvement loop
