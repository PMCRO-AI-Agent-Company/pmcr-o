---
asset_type: pmcro-reject-contract
version: "0.1.0"
target_skill: make
---

# Refusal Path: make step

## When to refuse

| Reject code | Condition |
|---|---|
| `missing-trail-id` | `--trail-id` was not supplied. |
| `missing-step-index` | `--step-index` was not supplied. |
| `trail-not-found` | No such trail exists. |
| `trail-sealed` | The trail is already sealed. |
| `no-plan-frame` | The trail's `plan.jsonl` has no PlanFrame to execute against. |
| `unknown-step-index` | `--step-index` doesn't match any `steps[].index` in the current PlanFrame. |

## Refusal steps

1. Do not append anything to `make.jsonl`.
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

- A refused invocation is distinct from a logged `result: "failed"` —
  refusal means nothing was attempted or logged at all; `failed` means the
  attempt happened and was recorded honestly.
