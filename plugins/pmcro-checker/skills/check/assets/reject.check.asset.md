---
asset_type: pmcro-reject-contract
version: "0.1.0"
target_skill: check
---

# Refusal Path: check run

## When to refuse

| Reject code | Condition |
|---|---|
| `missing-trail-id` | `--trail-id` was not supplied. |
| `trail-not-found` | No such trail exists. |
| `trail-sealed` | The trail is already sealed. |
| `no-plan-frame` | The trail's `plan.jsonl` has no PlanFrame with `success_criteria` to check against. |
| `no-make-steps` | The trail's `make.jsonl` has no MakeStep entries yet. |

## Refusal steps

1. Do not append anything to `check.jsonl`, and do not hand off to
   Reflector.
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
- Does not return a partial or best-guess verdict.
