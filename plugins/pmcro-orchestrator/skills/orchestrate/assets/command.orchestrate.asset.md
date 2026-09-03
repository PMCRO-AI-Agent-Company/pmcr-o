---
asset_type: pmcro-command-contract
version: "0.1.0"
target_skill: orchestrate
---

# Command Shape: orchestrate run

## Purpose

Open a governed cycle: mint-or-link a trail, log the orchestrator's own
entry into that trail's `plan.jsonl`, then hand off to Planner.

## Invocation

```text
/pmcro-orchestrator:orchestrate run [--trail-id <guid>] --task <task-id|seed> [--class A|B]
```

## Parameters

| Name | Required | Default | Meaning |
|---|---|---|---|
| `--task` | yes | — | A queue task id, or an inline seed-intent string. |
| `--trail-id` | no | mint a new one | Omit → Orchestrator mints a guid and calls `pmcro-trail:initialize` to materialize it. Supply → must already exist, be unsealed, and unlinked. |
| `--class` | no | `B` | Honoured only on the mint path (no `--trail-id`); ignored when linking to an existing trail. |

## Result (success)

```json
{
  "status": "ok",
  "trail_id": "<guid>",
  "task": "<task-id|seed>",
  "handed_off_to": "planner"
}
```

## Result (failure)

See `reject.orchestrate.asset.md` for refusal shapes and reject codes.

## Related Assets

- `run.orchestrate.asset.md` — accept-path implementation
- `reject.orchestrate.asset.md` — refusal-path implementation
- `pmcro-trail`'s `command.initialize.asset.md` — the trail materialization this calls into
