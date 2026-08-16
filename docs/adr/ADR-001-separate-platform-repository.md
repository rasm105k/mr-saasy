# ADR-001: MR SAAS'y lives outside product repositories

## Decision

MR SAAS'y is maintained as a separate platform repository.

## Reasoning

The platform must evolve independently from individual products.

Workslip is the first consumer, not the owner of the platform.

## Consequence

Products consume platform contracts. Platform does not import product domain models.
