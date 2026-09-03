---
asset_type: pmcro-reject-contract
version: "0.1.0"
target_skill: plan
---

# Refusal Path: plan run

## When to refuse

| Reject code | Condition |
|---|---|
| `missing-trail-id` | `--trail-id` was not supplied. |
| `trail-not-found` | No such trail exists. |
| `trail-sealed` | The trail is already sealed. |
| `no-orchestrate-frame` | The trail's `orchestrate.jsonl` has no entry — no cycle was opened to plan for. |
| `already-planned` | `plan.jsonl` already has a PlanFrame for this cycle's still-open task. |

## Refusal steps

1. Do not append anything to `plan.jsonl`, and do not hand off to Maker.
2. Identify the single matching reject code above (first match wins).
3. Return the refusal result shape below.

## Refusal result

```json
{
  "status": "rejected",
  "reject_code": "<one of the codes above>",
  "message": "<human-readable reason, no drive-letter or host-specific paths>"
}
```

## Non-goals

- Does not retry automatically.
- Does not fall back to a partial or best-guess PlanFrame.
