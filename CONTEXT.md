# PMCR-O Repository Context

- Repository: `pmcro-skills`
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
