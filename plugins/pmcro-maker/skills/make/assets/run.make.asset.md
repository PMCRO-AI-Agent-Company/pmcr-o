---
asset_type: pmcro-run-contract
version: "0.1.0"
target_skill: make
---

# Accept Path: make step

## Preconditions (all must hold, else reject)

1. `--trail-id` and `--step-index` supplied.
2. The trail exists, `sealed: false`.
3. `plan.jsonl` has a PlanFrame whose `steps[]` contains that `index`.

## Steps

1. **Read the step.** Load the matching `steps[]` entry (`action`,
   `subject_agent`) from the trail's current PlanFrame.
2. **Do the work.** Perform the action the step describes.
3. **Compose the MakeStep.** Record `step_index`, the `action` actually
   taken (which may differ in detail from the plan's description — record
   what really happened, not a restatement of the plan), and `result`
   (`ok`, `failed`, or `skipped`), conforming to
   `schema.trail-frame.asset.json`.
4. **Append.** Write one line to `make.jsonl`: `role: "maker"`,
   `type: "MakeStep"`, next monotonic `seq` for that file.
5. **Hand off.** If a step remains unattempted, hand off to Maker again
   for that step. Otherwise hand off to Checker.
6. Return the success result shape from `command.make.asset.md`.

## Non-goals

- Does not amend, reorder, or add to `plan.jsonl`.
- Does not decide whether the cycle as a whole succeeded.
- Does not retry a failed step automatically — a retry is a fresh,
  explicit `make step` call for the same `step-index`.
