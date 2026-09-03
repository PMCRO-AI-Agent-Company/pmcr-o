---
asset_type: pmcro-run-contract
version: "0.1.0"
target_skill: check
---

# Accept Path: check run

## Preconditions (all must hold, else reject)

1. `--trail-id` supplied.
2. The trail exists, `sealed: false`.
3. `plan.jsonl` has a PlanFrame with `success_criteria`.
4. `make.jsonl` has at least one MakeStep.

## Steps

1. **Read the evidence.** Load the PlanFrame's `success_criteria` and every
   MakeStep logged for this cycle.
2. **Evaluate each criterion independently** — against what actually
   happened (the MakeStep log, and, where relevant, direct re-inspection
   of the result), not against Maker's own `result` field taken at face
   value.
3. **Compose the CheckFrame.** For each criterion: `check` (the criterion
   text), `result` (`PASS`/`FAIL`), `evidence` (what was actually
   inspected). Then an overall `verdict` (`PASS` only if every criterion
   passed), conforming to `schema.trail-frame.asset.json`.
4. **Append.** Write one line to `check.jsonl`: `role: "checker"`,
   `type: "CheckFrame"`, next monotonic `seq` for that file.
5. **Hand off to Reflector.** Checker's job for this command ends here —
   it does not act on a FAIL itself.
6. Return the success result shape from `command.check.asset.md`.

## Non-goals

- Does not fix, redo, or amend any MakeStep.
- Does not decide the cycle's `disposition` — that is Reflector's read of
  this verdict, not Checker's own call.
- Does not seal the trail.
