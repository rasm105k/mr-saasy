# Infrastructure

Infrastructure is intentionally empty in foundation v0.1.

Rules before adding cloud resources:

- contracts and core must remain provider-neutral
- prefer OIDC/Managed Identity over static credentials
- environment isolation must be explicit
- no production credentials in local development
- no runtime dependency on deployment administrator credentials

Provider implementations (Azure, Keeper, GitHub, etc.) are added only behind explicit adapter contracts.
