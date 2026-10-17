# Cycle-2 merge checklist (authoring)

- [ ] Add ProjectName.Contracts + ProjectName.Runtime to ProjectName.slnx
- [ ] Reference Runtime from OrchestratorService + OrchestrationApi
- [ ] MapPmcroEndpoints + AddProjectNamePmcroRuntime in composition
- [ ] Keep AgentActivationGate as production activation (stub for tests only)
- [ ] Run ProjectName.Runtime.Tests on host with dotnet
- [ ] Do not set authority.register/grant/activate true in template defaults
