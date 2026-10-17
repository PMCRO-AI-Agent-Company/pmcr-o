# ProjectName AgentSystem

Generic template-driven multi-agent application built around PMCR-O governance, Microsoft Agent Framework, marketplaces, MCP, OpenAPI, console/TUI hosts, durable Trails, and cross-session messaging.

## Core projects

- `ProjectName.Contracts` — PMCR-O requests, grants, receipts, Trail/message contracts, executive organization contracts.
- `ProjectName.Core` — authority derivation, domain-independent application rules, repositories, unit of work, and message/seed abstractions.
- `ProjectName.Harness` — PMCR-O ingress, phase routing, lifecycle guards, Trail automation, dependency injection, and local test activation.
- `ProjectName.Marketplace` and `ProjectName.Capabilities` — marketplace discovery and governed capability resolution/invocation.
- `ProjectName.Runtime.Abstractions` and `ProjectName.Runtime.Maf` — runtime-neutral agent contract plus Microsoft Agent Framework implementation, Executive Council workflows, handoff, group chat, and checkpoint support.
- `ProjectName.Infrastructure` and `ProjectName.Adapters` — durable message transport and host/runtime adapters.
- `ProjectName.OrchestrationApi` — HTTP/OpenAPI projection.
- `ProjectName.McpServer` — dynamic MCP projection.
- `ProjectName.AgentCli` — plain text, JSON, and terminal UI projection.
- `ProjectName.ServiceDefaults` and `ProjectName.Tooling` — shared host defaults and tooling support.
- `ProjectName.Runtime` — compatibility composition surface.

## PMCR-O flow

New Trail: `@orchestrator /pmcro:init "<intent>"`

Existing Trail: `@orchestrator /pmcro:seed "<intent>"`

A governed cycle is:

ORCHESTRATE → PLAN → MAKE → CHECK → REFLECT

`PhaseRouter` validates the requested Frame and then dispatches through `IPhaseHandler`. A phase handler cannot create execution authority.

## Authority law

Discovery, projection, registration, grant, activation, invocation, and execution are distinct states.

Messages, Executive Council recommendations, organization manifests, workflow checkpoints, and model intelligence never grant authority.

Production activation must use the governed activation path. The local in-memory framework activation implementation is for tests and local development only.

## Executive Council

Executive offices are selected from organization manifests under `templates/organizations`.

Included examples:

- `startup.json`
- `bank.json`

The Executive Council supports concurrent analysis, bounded group chat, MAF handoff, human-decision request messages, and file-backed MAF checkpoint wiring. Executive advice remains non-authoritative.

## Hosts and projections

The same governed PMCR-O behavior can be projected through HTTP/OpenAPI, MCP, console, terminal UI, and MAF runtime adapters without changing authority semantics.

See `docs/MERGE_INTO_SOLUTION.md` for composition guidance.
