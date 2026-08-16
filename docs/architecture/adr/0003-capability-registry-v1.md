# ADR 0003 — Capability Registry v1

## Status
Proposed

## Context
MR SAAS'y needs a shared understanding of product capabilities across products, users, and AI agents.

Linear issues describe implementation work, but they are not a sufficient source of truth for AI reasoning because they are developer-oriented.

A capability must describe what users can achieve, who can access it, where it exists, and how agents may reason about it.

## Decision
Introduce Capability Registry as a platform capability.

Each capability must contain:

- stable capability id
- name
- purpose
- user roles
- permissions
- navigation locations
- UI entry points
- backend ownership
- frontend ownership
- documentation references
- audit requirements
- AI visibility rules

## Example

```yaml
id: audit_scope
name: Auditøradgang
category: Compliance
roles:
  - Admin
permission: jobs.audit_scope.manage
locations:
  - Job > Administration
```

## Agent usage

Agents consume capability context, not raw tickets.

Flow:

Linear
  ↓
Capability Context Builder
  ↓
AI Agent
  ↓
Permission Gateway
  ↓
Allowed Action

## Rules

- Capabilities must be searchable.
- Capabilities must be permission aware.
- Capabilities must be discoverable by users.
- Agents must not bypass capability permissions.
