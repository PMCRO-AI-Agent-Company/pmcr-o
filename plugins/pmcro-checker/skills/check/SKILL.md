---
name: check
description: Evaluates a cycle's MakeStep results against the PlanFrame's success_criteria and appends one CheckFrame with a verdict. USE FOR — independently verifying whether stated success criteria were actually met. DO NOT USE FOR — doing or redoing work, changing the plan, or closing the cycle.
metadata:
  version: "0.1.0"
  tier: VERIFICATION
  capability_class: PHASE
---

# Check

## Command

`/pmcro-checker:check run` — see `assets/command.check.asset.md` for the
full contract. This section is a pointer, not a duplicate.

## Purpose

Reads the trail's PlanFrame `success_criteria` and its logged MakeSteps,
evaluates each criterion against real evidence, and appends one CheckFrame
to `check.jsonl` with a PASS/FAIL verdict. Hands off to Reflector.

## Inputs / Outputs / Boundaries

See `assets/command.check.asset.md`, `assets/run.check.asset.md`, and
`assets/reject.check.asset.md`.

## References

- `assets/command.check.asset.md`
- `assets/run.check.asset.md`
- `assets/reject.check.asset.md`
- `scripts/New-CheckFrame.ps1` — the deterministic implementation of the
  accept path; call this rather than writing `check.jsonl` directly
- `references/README.md`
