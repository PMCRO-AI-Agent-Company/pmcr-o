---
asset_type: pmcro-run-contract
version: "0.1.0"
target_skill: initialize
---

# Accept Path: initialize

## Preconditions (all must hold, else reject)

1. `.pmcro/trails/` is present and writable.
2. If `--trail-id` was supplied: that folder does not already exist (mint
   path never overwrites) — supplying an existing id is a *link*, not a
   mint; see `reject.initialize.asset.md` for the link-time checks.
3. `--class` is `B`, or omitted (defaults to `B`). `A` is rejected — not
   implemented by this skill.

## Steps (mint path — no `--trail-id` supplied)

1. Mint a new guid — `trail_id`.
2. Create `.pmcro/trails/<trail_id>/`.
3. Write skeleton `plan.jsonl`, `make.jsonl`, `check.jsonl`, `reflect.jsonl`
   as empty files (zero lines — the caller appends frames as work happens;
   this skill never pre-writes placeholder content into them).
4. Write `disposition.json`:
   ```json
   {
     "trail_id": "<trail_id>",
     "trail_class": "B",
     "engine_generated": true,
     "opened_at": "<iso-8601 utc>",
     "disposition": "open",
     "sealed": false,
     "frame_schema": "plugins/pmcro-trail/skills/initialize/assets/schema.trail-frame.asset.json"
   }
   ```
5. Return the success result shape from `command.initialize.asset.md`.

## Steps (link path — `--trail-id <guid>` supplied and the folder exists)

1. Read `disposition.json` for that trail.
2. Verify `sealed: false` and `disposition: "open"` — else reject
   (`trail-sealed` / `trail-linked`).
3. Return the same success result shape, `engine_generated` unchanged.

## Non-goals

- Does not decide *whether* a cycle should open — that is
  `pmcro-orchestrator:orchestrate`'s job.
- Does not write PlanFrame/MakeStep/etc. content into the phase files —
  callers append their own frames, each conforming to
  `schema.trail-frame.asset.json`.
- Does not seal trails (that is a `pmcro-trail` capability not yet built —
  flagged as a follow-up seed, not invented here).

## Implementation

`scripts/New-Trail.ps1` implements the mint path deterministically (zero
reasoning — file mechanics only). The link path is a plain read + check, no
script required.
