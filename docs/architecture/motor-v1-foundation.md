# MOTOR v1 foundation

MOTOR is MR SAAS'y's orchestration engine. MOTOR-001 provides contracts, deterministic
policies, reference stores and safety gates; it does not run autonomous production work.

## Runtime shape

```text
Mission
  -> Agent Router / Registry
  -> Model Router
  -> MCP Gateway
  -> Permission + Approval Check
  -> Connector / Tool
  -> Events
  -> Memory
  -> Learning records
```

The gateway is the trust boundary. Agents never receive Azure, GitHub, Linear or Workslip
clients directly. Tool argument values are forwarded to a connector but are not copied to
events or memory.

## Core domain

`Mission`, `AgentDefinition`, `ModelSelection`, `ToolCall`, `Approval`, `MotorEvent` and
`LearningRecord` use stable MOTOR-owned identifiers and explicit project context. The
project context includes workspace, project, environment and optional customer identity,
so routing cannot silently cross projects or customers.

## Agent registry

The default catalog contains Gordon, Forge, QA, Cleanup Guardian, Security Guardian and
Data Guardian. Each definition declares role, capabilities, exact permissions and approval
requirements. Gordon is read-only by default. Forge may write feature branches and create
pull requests, but protected-branch writes and merges require approval. Cleanup, permission,
secret and data mutations are explicitly governed.

## Model routing and evidence

The router consumes current candidates, token estimates and historical performance. A
selection records logical model, provider, reason, estimated cost and evaluation flag in
`IModelSelectionLog`. Execution metadata is linked later through `ModelExecutionResult`.
Provider model SKUs and prices are configuration, not contract constants.

## Events

MOTOR-001 defines:

- `MissionStarted`
- `AgentAssigned`
- `ModelSelected`
- `ToolCalled`
- `DecisionMade`
- `ActionSuggested`
- `ActionApproved`
- `ActionCompleted`
- `LearningCreated`

Every event has an event ID, mission ID, project context, UTC occurrence time, correlation
ID and schema version. `MotorEventEnvelope` flattens routing dimensions while retaining a
typed JSON payload, preparing later EventStoreDB, Data Lake, Power BI and ML adapters.

## Memory and learning

`IMotorMemoryStore` separates decisions, reusable solutions, agent performance, model
performance, business impact and learning. Learning links the action, observed result and
human feedback. MOTOR-001 intentionally does not perform training or autonomous policy
updates from these records.

## Safe deployment

`SafeDeploymentGate` is fail-closed and pure: it cannot deploy. Readiness requires evidence
for plan, validation, Azure What-If and bounded cost, followed by a current approval bound
to the mission, project, exact action reference and exact What-If evidence. A future Azure MCP adapter must execute
only after this gate and emit completion evidence.

The Bicep entry point is `infrastructure/bicep/main.bicep`. Identity, Key Vault, Service Bus,
Application Insights/Log Analytics and Data Lake Gen2 are separate modules. Every module is
disabled by default and no parameter file enables resources in MOTOR-001.

## Explicitly deferred

- live MCP connectors and authentication flows;
- model/provider execution, including OpenCode ZEN invocation;
- EventStoreDB, MassTransit and Polly adapters;
- production memory, analytics exports and ML training;
- Azure role assignments, secrets, private endpoints and deployment execution;
- UI/dashboard work and Workslip product changes.
