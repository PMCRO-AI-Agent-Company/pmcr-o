---
name: plan
description: Turns an opened cycle's task into a single PlanFrame — goal, ordered steps naming their subject_agent, success_criteria, out_of_scope. USE FOR — deciding what will happen and who does it, before any work starts. DO NOT USE FOR — executing steps, verifying results, or closing the cycle.
metadata:
  version: "0.1.0"
  tier: PLANNING
  capability_class: PHASE
---

# Plan

## Command

`/pmcro-planner:plan run` — see `assets/command.plan.asset.md` for the full
contract. This section is a pointer, not a duplicate.

## Purpose

Reads the trail's `orchestrate.jsonl` entry for the task, and appends
exactly one PlanFrame to `plan.jsonl`: goal, ordered steps (each naming its
`subject_agent`), `success_criteria`, `out_of_scope`. Hands off to Maker.

## Inputs / Outputs / Boundaries

See `assets/command.plan.asset.md`, `assets/run.plan.asset.md`, and
`assets/reject.plan.asset.md`.

## References

- `assets/command.plan.asset.md`
- `assets/run.plan.asset.md`
- `assets/reject.plan.asset.md`
- `references/README.md`
