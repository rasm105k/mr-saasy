# Infrastructure

MOTOR-001 adds an opt-in Azure Bicep foundation under `bicep/`. The entry point
defaults all resource modules to disabled, so the default deployment creates nothing.

Rules before adding cloud resources:

- contracts and core must remain provider-neutral
- prefer OIDC/Managed Identity over static credentials
- environment isolation must be explicit
- no production credentials in local development
- no runtime dependency on deployment administrator credentials

Provider implementations (Azure, Keeper, GitHub, etc.) are added only behind explicit adapter contracts.

See `bicep/README.md` for the safe deployment sequence and explicit non-scope.
