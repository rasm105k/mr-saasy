# MOTOR Azure foundation

This folder contains opt-in Bicep modules for identity, security, messaging,
monitoring and data. `main.bicep` defaults every `enable*` parameter to `false`,
so a default deployment creates no Azure resources.

If any module is enabled, resource materialization also requires an explicit
`changeReference`; resources remain conditionally blocked while the `unapproved-plan`
default is present. This is audit metadata, not a substitute for the external approval gate.

Bicep is a declarative plan, not the approval boundary. A future deployment adapter
must follow MOTOR's gate in this order:

1. create plan evidence;
2. validate the template;
3. run Azure What-If and retain its evidence reference;
4. obtain a bounded cost estimate;
5. obtain approval bound to the mission, project and exact action reference;
6. deploy through the MCP Gateway;
7. emit `ActionCompleted` and preserve deployment evidence.

No deploy script, environment parameter file, role assignment, secret value, private
endpoint or production resource is included in MOTOR-001. Enabling a module for
What-If must not be reused as authorization to run an Azure deployment command.
