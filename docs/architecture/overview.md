# MR SAAS'y Architecture Overview

## Vision

MR SAAS'y is the enterprise control plane for MR Software products.

## Boundary

MR SAAS'y owns platform capabilities:

- Identity
- Tenants
- Applications
- Capabilities
- Security
- Compliance
- Operations

Products own:

- Domain workflows
- Customer business data
- Product-specific logic

## Dependency direction

```
MR SAAS'y Platform
        ^
        |
 Contracts / APIs
        ^
        |
 Products
```

MR SAAS'y must not depend on product domain models.
