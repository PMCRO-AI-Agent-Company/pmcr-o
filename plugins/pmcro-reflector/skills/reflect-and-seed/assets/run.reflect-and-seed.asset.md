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
3. **Compose the Reflection.** `content` (a real narrative of what was
   learned, not a restatement of the CheckFrame), `disposition`, and
   `next_seed` (an object per `schema.trail-frame.asset.json`'s
   `next_seed` field, or `null` if nothing follows).
4. **Append.** Write one line to `reflect.jsonl`: `role: "reflector"`,
   `type: "Reflection"`, next monotonic `seq` for that file.
5. **File the seed, if any.** When `next_seed` is non-null, write one file
   under `.pmcro/queue/` conforming to `schema.queue-item.asset.json`
   (`status: "open"`, `source_trail_id` set to this trail).
6. **Seal.** Update this trail's `disposition.json`: `sealed: true`,
   `disposition` set to the value chosen in step 2, `sealed_at` set.
7. Return the success result shape from `command.reflect-and-seed.asset.md`.

## Non-goals

- Does not re-run or second-guess Checker's verdict.
- Does not open the next cycle for a seed it just filed — that is a later,
  separate Orchestrator decision.
- Does not unseal or amend an already-sealed trail.
