---
name: make
description: Executes exactly one PlanFrame step and appends a MakeStep recording what actually happened. USE FOR — doing the work a plan step names, one step at a time. DO NOT USE FOR — deciding the plan, judging overall success, or closing the cycle.
metadata:
  version: "0.1.0"
  tier: EXECUTION
  capability_class: PHASE
---

# Make

## Command

`/pmcro-maker:make step` — see `assets/command.make.asset.md` for the full
contract. This section is a pointer, not a duplicate.

## Purpose

Reads one step out of the trail's current PlanFrame, performs it, and
appends exactly one MakeStep line to `make.jsonl` recording what actually
happened. Hands off to itself for the next unattempted step, or to Checker
once none remain.

## Inputs / Outputs / Boundaries

See `assets/command.make.asset.md`, `assets/run.make.asset.md`, and
`assets/reject.make.asset.md`.

## References

- `assets/command.make.asset.md`
- `assets/run.make.asset.md`
- `assets/reject.make.asset.md`
- `references/README.md`
