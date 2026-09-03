---
asset_type: pmcro-reject-contract
version: "0.1.0"
target_skill: reflect-and-seed
---

# Refusal Path: reflect-and-seed run

## When to refuse

| Reject code | Condition |
|---|---|
| `missing-trail-id` | `--trail-id` was not supplied. |
| `trail-not-found` | No such trail exists. |
| `trail-already-sealed` | The trail is already sealed — nothing to close. |
| `no-check-frame` | The trail's `check.jsonl` has no CheckFrame verdict to reflect on. |

## Refusal steps

1. Do not append anything to `reflect.jsonl`, do not file a queue item,
   and do not seal the trail.
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
- Does not seal a trail as a side effect of a refusal — sealing only
  happens on the accept path, as its very last step.
