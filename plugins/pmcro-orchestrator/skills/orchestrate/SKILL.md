---
name: orchestrate
description: Sole cycle-dispatch capability. Claims or accepts a task, mints-or-links a trail via pmcro-trail, logs its own OrchestratorFrame, then hands off to Planner. USE FOR — starting a governed cycle. DO NOT USE FOR — planning, making, checking, reflecting, or trail content beyond its own frame.
metadata:
  version: "0.1.0"
  tier: GOVERNANCE
  capability_class: KERNEL
---

# Orchestrate

## Command

`/pmcro-orchestrator:orchestrate run` — see `assets/command.orchestrate.asset.md`
for the full contract. This section is a pointer, not a duplicate.

## Purpose

The only skill that opens a cycle. Uses the **convenient** trail-id contract:
mints and materializes a new trail (via `pmcro-trail:initialize`) when none
is supplied, or links to an existing, unsealed, unlinked one when it is.

## Inputs / Outputs / Boundaries

See `assets/command.orchestrate.asset.md`, `assets/run.orchestrate.asset.md`,
and `assets/reject.orchestrate.asset.md`.

## References

- `assets/command.orchestrate.asset.md`
- `assets/run.orchestrate.asset.md`
- `assets/reject.orchestrate.asset.md`
- `scripts/New-OrchestrateFrame.ps1` — the deterministic implementation of
  the claim/link step; call this rather than writing `orchestrate.jsonl`
  directly
- `references/README.md`
