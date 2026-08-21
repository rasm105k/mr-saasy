# ADR-006: Bicep is the Infrastructure-as-Code standard

## Status
Accepted

## Decision

MR SAAS'y and Workslip use Bicep for declarative Azure and Entra infrastructure. Do not introduce Terraform or Pulumi as a parallel Infrastructure-as-Code stack.

PowerShell remains the orchestration layer for ordered deployment operations that are imperative rather than resource declarations.

## Reasoning

At the time of this decision, the Workslip infrastructure baseline was approximately 1,501 lines of Bicep and 3,355 lines of PowerShell. Replacing Bicep would therefore replace only the declarative part of the deployment system; the imperative PowerShell would still be required.

That PowerShell owns operations such as secret generation and rotation, Key Vault and App Configuration reconciliation, Entra/Graph orchestration, Azure SQL principal provisioning through `sqlcmd`, temporary firewall management and local Entra state. Those responsibilities do not become declarative by changing the IaC language underneath them.

The existing Bicep deployment also uses `extension microsoftGraphV1` for Microsoft Graph resources, so the current declarative boundary already covers the required Azure and Entra resource definitions.

Plan-before-apply safety is handled within the existing Bicep deployment flow through non-mutating planning/What-If support; it does not justify a second IaC toolchain.

## Consequence

- New declarative Azure and Entra infrastructure is implemented in Bicep.
- Imperative deployment operations remain in focused PowerShell scripts.
- The same environment must not be maintained in parallel Bicep and Terraform/Pulumi definitions.
- Introducing Terraform or Pulumi requires a new ADR that explicitly supersedes this decision.

## References

- Linear WOR-699
- Workslip `src/BE/infrastructure/main.bicep`
- Workslip `src/BE/infrastructure/README.md`
