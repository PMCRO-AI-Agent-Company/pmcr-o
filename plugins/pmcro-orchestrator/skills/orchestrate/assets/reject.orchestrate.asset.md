---
asset_type: pmcro-reject-contract
version: "0.1.0"
target_skill: orchestrate
---

# Refusal Path: orchestrate run

## When to refuse

| Reject code | Condition |
|---|---|
| `missing-task` | `--task` was not supplied. |
| `trail-not-found` | `--trail-id` supplied but no such trail exists. |
| `trail-sealed` | The supplied trail is already sealed. |
| `trail-linked` | The supplied trail is already bound to another active cycle. |
| `mint-failed` | `pmcro-trail:initialize` (mint path) returned an error. |

## Refusal steps

1. Do not mint or link any trail, and do not hand off to Planner.
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
- Does not propose a next seed.
