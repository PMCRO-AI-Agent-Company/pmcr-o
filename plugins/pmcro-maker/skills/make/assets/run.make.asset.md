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
3. **Compose what actually happened.** Reason out the `action` actually
   taken (which may differ in detail from the plan's description — record
   what really happened, not a restatement of the plan) and the `result`
   (`ok`, `failed`, or `skipped`).
4. **Call `scripts/New-MakeStep.ps1`** with `step_index`, `action`, and
   `result`, rather than writing to `make.jsonl` directly. The script
   re-checks every precondition above, computes the next monotonic `seq`,
   stamps `ts` / `role: "maker"` / `type: "MakeStep"`, appends one
   correctly schema-shaped line, and works out whether any PlanFrame step
   is still unattempted.
5. **Hand off** using the script's returned `handed_off_to` — `maker`
   (itself, for the next unattempted step) or `checker`.
6. Return the script's own JSON output — it already matches the success
   result shape in `command.make.asset.md`.

## Non-goals

- Does not amend, reorder, or add to `plan.jsonl`.
- Does not decide whether the cycle as a whole succeeded.
- Does not retry a failed step automatically — a retry is a fresh,
  explicit `make step` call for the same `step-index`.
