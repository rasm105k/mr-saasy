# ADR-004: No shared database

## Decision

MR SAAS'y and products do not share databases.

Allowed:

```
Platform
  |
 Contracts / APIs
  |
 Product adapters
```

Forbidden:

```
Platform -> Product database
```
