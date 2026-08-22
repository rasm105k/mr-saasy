# MOTOR Workflow Engine

## Purpose

MOTOR workflows represent durable business and engineering processes.

A mission is not a single request. It is a stateful process that can survive interruptions.

## Workflow lifecycle

Example:

Analyse
 -> Plan
 -> Execute
 -> Validate
 -> Approve
 -> Complete

## Required properties

- durable state
- event history
- retry policies
- timeout handling
- compensation actions
- replay capability

## Future implementation boundary

The workflow engine should expose contracts independent of execution providers.
EventStoreDB remains the long-term persistence direction.
MassTransit handles messaging boundaries.
Polly handles resilience policies.
