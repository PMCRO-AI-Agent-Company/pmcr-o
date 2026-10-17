# ProjectName Local Agent Instructions

This project is a local AgentSystem instance generated from the global `agentsystem` template.

## Session bootstrap

Before PMCR-O work, load `.agents/session-bootstrap.json`. It defines the canonical startup read order, messy-Seed ingress rules, True Intent semantics, and activation boundaries for new sessions and IDE hosts.

## Inheritance

- Global reusable configuration: `D:\AgentSystem\global\.agents`
- Global marketplaces: `D:\AgentSystem\global\marketplaces`
- Global templates/tooling/runtimes/harnesses/products: `D:\AgentSystem\global\...`
- Project-local configuration: `.agents/`
- Project-local PMCR-O state: `.pmcro/`

Project-local configuration may narrow inherited behavior and authority but may never widen it.

## Clean Architecture

Use `ProjectName.Domain`, `ProjectName.Application.Abstractions`, `ProjectName.Infrastructure`, contracts, capabilities, harness, runtimes, adapters, and host/API projects with dependencies pointing inward. Prefer .NET/BCL and established libraries over bespoke infrastructure.

## PMCR-O ingress

Start a new Trail with `@orchestrator /pmcro:init "<initial intent>"`. INIT opens Cycle 1 but is not a cognitive Frame. The first Frame is `@orchestrator /pmcro:orchestrate`. Later intent uses `/pmcro:seed`. Bare operational prose is rejected with the corrected canonical route. Messy Seed content is valid; True Intent is earned at REFLECT after PLAN, MAKE, and CHECK establish resource-backed reality.

## Activation

Discovery, projection, registration, and grant are distinct from activation. Projected skills may be visible to an IDE without being executable. Framework activation is explicit per host and must remain within the effective grant ceiling.
