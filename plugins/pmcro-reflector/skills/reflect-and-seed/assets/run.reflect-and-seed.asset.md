---
asset_type: pmcro-run-contract
version: "0.1.0"
target_skill: reflect-and-seed
---

# Accept Path: reflect-and-seed run

## Preconditions (all must hold, else reject)

1. `--trail-id` supplied.
2. The trail exists, `sealed: false`.
3. `check.jsonl` has a CheckFrame with a `verdict` to reflect on.

## Steps

1. **Read the verdict.** Load `check.jsonl`'s CheckFrame `verdict` (and,
   for context, the PlanFrame's `goal` and the MakeStep log).
2. **Decide the disposition.** `done` (verdict PASS, nothing further
   needed), `blocked` (verdict FAIL and no further Make attempt is
   warranted in this cycle), `superseded` (the goal no longer applies),
   or `informational` (the cycle wasn't pass/fail-shaped work).
3. **Compose the Reflection's content.** `content` (a real narrative of
   what was learned, not a restatement of the CheckFrame) and, if
   anything follows, a `next_seed` (`summary`, `proposed_role`) plus an
   id to file it under.
4. **Call `scripts/Complete-ReflectAndSeed.ps1`** with `content`,
   `disposition`, and (when there is one) the `next_seed` and its id,
   rather than writing `reflect.jsonl`, `.pmcro/queue/`, or
   `disposition.json` directly. The script re-checks every precondition
   above, computes the next monotonic `seq`, stamps `ts` /
   `role: "reflector"` / `type: "Reflection"`, appends the line, files a
   `schema.queue-item.asset.json`-conformant queue item when a seed was
   given, and seals the trail — all as one atomic, deterministic
   operation. Reasoning stays with the caller (was the cycle actually
   done, what should follow); the file mechanics do not.
5. Return the script's own JSON output — it already matches the success
   result shape in `command.reflect-and-seed.asset.md`.

## Non-goals

- Does not re-run or second-guess Checker's verdict.
- Does not open the next cycle for a seed it just filed — that is a later,
  separate Orchestrator decision.
- Does not unseal or amend an already-sealed trail.
