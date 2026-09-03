---
asset_type: pmcro-command-contract
version: "0.1.0"
target_skill: plan
---

# Command Shape: plan run

## Purpose

Turn the task Orchestrator opened this cycle for into a single PlanFrame,
appended to `plan.jsonl`, then hand off to Maker.

## Invocation

```text
/pmcro-planner:plan run --trail-id <guid>
```

## Parameters

| Name | Required | Default | Meaning |
|---|---|---|---|
| `--trail-id` | yes | — | The trail Orchestrator already opened (or linked). Must exist, be unsealed, and have an `orchestrate.jsonl` entry to plan from. |

The PlanFrame's own content (`goal`, `steps`, `success_criteria`,
`out_of_scope`, and optionally `chosen_name` / `name_rationale`) is not a
CLI flag — Planner composes it per
`plugins/pmcro-trail/skills/initialize/assets/schema.trail-frame.asset.json`
and appends it directly. This contract does not re-specify that shape.

## Result (success)

```json
{
  "status": "ok",
  "trail_id": "<guid>",
  "phase": "plan",
  "seq": 1,
  "handed_off_to": "maker"
}
```

## Result (failure)

See `reject.plan.asset.md` for refusal shapes and reject codes.

## Related Assets

- `run.plan.asset.md` — accept-path implementation
- `reject.plan.asset.md` — refusal-path implementation
- `pmcro-orchestrator`'s `run.orchestrate.asset.md` — the predecessor step
- `pmcro-maker`'s `command.make.asset.md` — the successor step
