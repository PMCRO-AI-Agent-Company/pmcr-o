# PMCR-O Repository Context

- Repository: `pmcr-o` (https://github.com/PMCRO-AI-Agent-Company/pmcr-o)
- Architecture: PMCRO (Planner, Maker, Checker, Reflector, Orchestrator + Trail)
- Control Plane: ColonyRuntime v1 (`.pmcro/manifest.yaml`)

## Role contracts

- **Orchestrator** — sole routing/trail-binding authority; owns goal state, lease, and phase transitions; performs no domain code changes.
- **Planner** — turns claimed seed intents into testable PlanFrames.
- **Maker** — implements bounded by the PlanFrame.
- **Checker** — validates artifact deltas against PlanFrame criteria and must pass before seal.
- **Reflector** — analyzes history, updates memory, seals `disposition.json`, and seeds subsequent tasks.
- **Trail** — materializes and validates Class-B trail files.

## Runtime boundaries

The deterministic queue/engine provides file-backed intake and lifecycle mechanics only. It does not replace the role contracts or create a parallel model orchestration engine.

`.pmcro/` is the authoritative colony runtime. Plugin source remains under `plugins/`; maintainer tooling remains under `.agents/`.

## AppHost lifecycle

Any live verification of `ProjectName.AppHost` (Runtime, OrchestrationApi, Ollama)
uses the Aspire CLI, never `dotnet run` plus manual process termination:
`aspire start` (auto-detects and stops a stray prior instance), `aspire wait
<resource>`, `aspire stop`. This is mandatory, not a style preference --
`dotnet run` plus killing processes by hand is a documented cause of AppHost
file-lock build failures (MSB3027/MSB3021/MSB3491; see
[microsoft/aspire#15801](https://github.com/microsoft/aspire/issues/15801)),
and this repo hit exactly those errors before adopting the CLI pattern.
Known deviation: trails `eb6c47d4-817e-4004-bd76-3242cce889ef` and
`d360b692-5014-4267-9018-9b94758e9170` used `dotnet run` plus manual process
termination, before this convention was adopted in trail
`1de7efcb-bff0-4e7f-a9ae-7754399a0971`; their evidence stands as recorded and
is not retroactively edited, per the immutability rule in `.pmcro/AGENTS.md`.
See `plugins/pmcro-aspire/agents/aspire.md` for the full command catalog.
