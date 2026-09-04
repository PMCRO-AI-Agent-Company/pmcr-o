---
asset_type: pmcro-run-contract
version: "0.1.0"
target_skill: plan
---

# Accept Path: plan run

## Preconditions (all must hold, else reject)

1. `--trail-id` supplied.
2. The trail exists, `sealed: false`.
3. The trail's `orchestrate.jsonl` has at least one entry (a cycle was
   actually opened) to plan from.
4. `plan.jsonl` has no PlanFrame yet for the task this cycle is currently
   working (re-planning an already-planned, still-open cycle is not
   supported by this command — that would be a new cycle).

## Steps

1. **Read the task.** Load the trail's `orchestrate.jsonl` entry to learn
   what Orchestrator opened this cycle for.
2. **Compose the PlanFrame's content.** Reason out `goal`, ordered `steps`
   (each with `index`, `action`, `subject_agent`), `success_criteria`,
   `out_of_scope`, and — only if this cycle is naming a new work product —
   `chosen_name` and `name_rationale`.
3. **Call `scripts/New-PlanFrame.ps1`** with that content, rather than
   writing to `plan.jsonl` directly. The script is the deterministic,
   zero-reasoning implementation of this accept path: it re-checks every
   precondition above, computes the next monotonic `seq`, stamps `ts` /
   `role: "planner"` / `type: "PlanFrame"`, and appends one correctly
   schema-shaped line. Reasoning stays with the caller (what the plan
   *is*); the file mechanics (does it validate, where does it go, what
   `seq` is next) do not.
4. **Hand off to Maker** for the first step, using the script's returned
   `handed_off_to`. Planner's job for this command ends here.
5. Return the script's own JSON output — it already matches the success
   result shape in `command.plan.asset.md`.

## Non-goals

- Does not execute any step — every step names its own `subject_agent`.
- Does not write MakeStep, CheckFrame, or Reflection entries.
- Does not seal the trail or judge whether the cycle ultimately succeeded.
