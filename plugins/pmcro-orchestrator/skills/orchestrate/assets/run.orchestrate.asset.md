---
asset_type: pmcro-run-contract
version: "0.1.0"
target_skill: orchestrate
---

# Accept Path: orchestrate run

## Preconditions (all must hold, else reject)

1. `--task` supplied.
2. If `--trail-id` supplied: the trail exists, `sealed: false`, and
   `disposition: "open"` and not already bound to another active cycle.
3. No unresolved recovery condition is known to block a new cycle (none
   defined yet in this repo — nothing to check today; not invented ahead of
   being asked for).

## Steps

1. **Mint-or-link.**
   - No `--trail-id`: call `pmcro-trail:initialize --class <class, default B>`
     → get back a fresh `trail_id`.
   - `--trail-id` supplied: call `pmcro-trail:initialize --trail-id <id>`
     (link path) → verified, same `trail_id` back.
2. **Log the orchestrator's own frame** — append one line to that trail's
   `plan.jsonl`, conforming to `schema.trail-frame.asset.json`
   (`role: "orchestrator"`, `type: "MessySeedIntent"` or a dispatch-note
   type as appropriate), recording the task claimed/received and the
   decision to open this cycle.
3. **Hand off to Planner.** Orchestrator's job for this command ends here —
   it does not continue into Plan/Make/Check/Reflect itself.
4. Return the success result shape from `command.orchestrate.asset.md`.

## Non-goals

- Does not write PlanFrame, MakeStep, CheckFrame, or Reflection entries —
  those belong to Planner, Maker, Checker, Reflector respectively, each
  appending to their own phase file.
- Does not seal the trail.
- Does not pick the next Seed Intent.
