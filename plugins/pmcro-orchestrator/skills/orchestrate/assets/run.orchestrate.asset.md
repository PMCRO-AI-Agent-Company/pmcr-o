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
   - No `--trail-id`: call `pmcro-trail`'s `scripts/New-Trail.ps1
     -PmcroRoot ... -Class <class, default B>` → get back a fresh
     `trail_id`.
   - `--trail-id` supplied: skip minting — the link-path verification
     happens as part of step 2 below, since claiming and verifying an
     existing trail are the same operation.
2. **Call `scripts/New-OrchestrateFrame.ps1`** with that `trail_id` and
   the task, rather than writing to `orchestrate.jsonl` directly. The
   script re-checks every precondition (trail exists, unsealed, not
   already claimed by a prior cycle — this check *is* the link-path
   verification for a supplied `--trail-id`), stamps `ts` /
   `role: "orchestrator"` / `type: "MessySeedIntent"`, and appends one
   correctly schema-shaped line recording the task claimed/received.
3. **Hand off to Planner.** Orchestrator's job for this command ends here —
   it does not continue into Plan/Make/Check/Reflect itself.
4. Return the script's own JSON output — it already matches the success
   result shape in `command.orchestrate.asset.md`.

## Non-goals

- Does not write PlanFrame, MakeStep, CheckFrame, or Reflection entries —
  those belong to Planner, Maker, Checker, Reflector respectively, each
  appending to their own phase file.
- Does not seal the trail.
- Does not pick the next Seed Intent.
