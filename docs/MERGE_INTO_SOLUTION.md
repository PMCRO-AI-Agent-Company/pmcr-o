# Compose AgentSystem into a host solution

The template is already a complete modular AgentSystem. Use this guide when composing its projects into a larger product or Aspire solution.

## Composition

1. Reference the projects needed by the host from `ProjectName.slnx` rather than copying only the old Contracts/Runtime slice.
2. Register `AddProjectNameAgentSystem(...)` or `AddProjectNamePmcroRuntime(...)` from the Harness composition surface.
3. Map `MapPmcroEndpoints()` in the HTTP host when the OpenAPI projection is required.
4. Run `ProjectName.McpServer` when dynamic MCP projection is required.
5. Use `ProjectName.AgentCli` for plain text, JSON, or terminal UI access.
6. Configure `messageBusRoot` when durable cross-session Seed and message delivery is required.
7. Register the selected `IAgentRuntime` implementation when model-backed phase work is required.

## Phase execution

`PhaseRouter` validates the PMCR-O request and Frame before dispatching to `IPhaseHandler`.

The default handler is intentionally non-privileged. A model-backed or capability-backed handler must receive verified authority through the governed runtime boundary; it must not derive authority from the phase request itself.

## Microsoft Agent Framework

`ProjectName.Runtime.Maf` contains the MAF runtime and Executive Council workflow projections.

Supported Executive Council patterns include concurrent analysis, bounded group chat, handoff, human-decision request bridging, and file-backed workflow checkpointing.

Organization shape comes from `templates/organizations/*.json`. Loading a manifest selects executive offices only; it does not register, grant, activate, or authorize them.

## Marketplace and capability law

A capability is not executable merely because it was discovered or projected.

Keep these states distinct:

DISCOVER → PROJECT → REGISTER → GRANT → ACTIVATE → INVOKE → EXECUTE

A governed capability must be present in the marketplace, have a declared entrypoint, pass runtime preflight, and be covered by the effective PMCR-O grant before execution.

## Activation

The Harness contains an in-memory local activation implementation for tests and local development.

Production composition must use the durable governed activation gate and independently checked receipts. Do not enable registration, grants, or activation by template default.

## Cross-session work

Use the durable message bus for Seed candidates, Reflector next Seeds, resume messages, executive requests, evidence requests, delegation proposals, and human-decision requests.

Message arrival is not a phase transition and never grants authority. A fresh session must re-enter PMCR-O admission before continuing work.

## Host adapters

Host adapters may report host facts and expose transport/runtime mechanics. They do not own PMCR-O authority fields and must not widen inherited authority.
