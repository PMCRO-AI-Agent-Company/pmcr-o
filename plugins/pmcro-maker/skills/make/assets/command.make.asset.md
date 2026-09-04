---
asset_type: pmcro-command-contract
version: "0.1.0"
target_skill: make
---

# Command Shape: make step

## Purpose

Execute one step from the trail's current PlanFrame and append one
MakeStep line to `make.jsonl` recording what actually happened.

## Invocation

```text
/pmcro-maker:make step --trail-id <guid> --step-index <n>
```

## Parameters

| Name | Required | Default | Meaning |
|---|---|---|---|
| `--trail-id` | yes | — | The trail whose current PlanFrame is being executed. Must exist, be unsealed, and have a PlanFrame. |
| `--step-index` | yes | — | Must match a `steps[].index` in that PlanFrame. |

The MakeStep's own content (`action`, `result`) is composed by Maker per
`schema.trail-frame.asset.json` after actually performing the step — not a
CLI flag.

## Result (success)

```json
{
  "status": "ok",
  "trail_id": "<guid>",
  "phase": "make",
  "seq": 1,
  "step_index": 1,
  "result": "ok",
  "handed_off_to": "checker"
}
```

`result` may also be `"failed"` or `"skipped"` — those are still a
successful *command* invocation (the attempt was logged); see
`reject.make.asset.md` only for cases where nothing could be logged at all.

## Result (failure)

See `reject.make.asset.md` for refusal shapes and reject codes.

## Related Assets

- `run.make.asset.md` — accept-path implementation
- `reject.make.asset.md` — refusal-path implementation
- `pmcro-planner`'s `run.plan.asset.md` — the predecessor step
- `pmcro-checker`'s `command.check.asset.md` — the successor step
