# PMCR-O Repository Agent Contract

## Prime invariants

1. Read `laws.md` and `CONTEXT.md` before claiming or executing mutations.
2. Orchestrator-only dispatch: never jump into Maker/Planner without an active linked trail.
3. Atomic File Protocol EC-SYS-001: every file output is complete in one operation; no partial edits, truncation, or TODO placeholders.
4. TYPE1 mutation safety: persistent filesystem writes, deletion, and external network calls require a certified PlanFrame and CheckFrame verification.

## Lifecycle commands

- `/pmcro-orchestrator:orchestrate run [--trail-id <guid>] --task <task-id|seed> [--class A|B]`
- `/pmcro-trail:initialize run [--trail-id <guid>] [--class B]`
- `/pmcro-planner:plan run --trail-id <guid>`
- `/pmcro-maker:make run --trail-id <guid>`
- `/pmcro-checker:check run --trail-id <guid>`
- `/pmcro-reflector:reflect-and-seed run --trail-id <guid>`

## Boundaries

`.pmcro/` is authoritative colony runtime; `.agents/` is maintainer authoring; `plugins/` are shipped product plugins.

Class-B trails are mandatory for multi-step cycles. A trail seals only when `check.jsonl` passes and `disposition.json` has `"sealed": true`.
Near context exhaustion, Reflector commits durable state and seeds the next cycle rather than truncating evidence.
