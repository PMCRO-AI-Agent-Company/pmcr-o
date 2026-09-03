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
2. **Compose the PlanFrame.** Build `goal`, ordered `steps` (each with
   `index`, `action`, `subject_agent`), `success_criteria`, `out_of_scope`,
   and — only if this cycle is naming a new work product — `chosen_name`
   and `name_rationale`, all conforming to
   `schema.trail-frame.asset.json`.
3. **Append.** Write one line to `plan.jsonl`: `role: "planner"`,
   `type: "PlanFrame"`, next monotonic `seq` for that file.
4. **Hand off to Maker** for the first step. Planner's job for this command
   ends here.
5. Return the success result shape from `command.plan.asset.md`.

## Non-goals

- Does not execute any step — every step names its own `subject_agent`.
- Does not write MakeStep, CheckFrame, or Reflection entries.
- Does not seal the trail or judge whether the cycle ultimately succeeded.
