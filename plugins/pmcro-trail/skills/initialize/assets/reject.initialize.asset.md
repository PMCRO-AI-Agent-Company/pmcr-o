---
asset_type: pmcro-reject-contract
version: "0.1.0"
target_skill: initialize
---

# Refusal Path: initialize

## When to refuse

| Reject code | Condition |
|---|---|
| `class-a-unsupported` | `--class A` was requested — not implemented by this skill. |
| `trail-not-found` | `--trail-id` supplied on the **link** path but no such folder exists under `.pmcro/trails/`. |
| `trail-already-exists` | `--trail-id` supplied on the **mint** path (via `New-Trail.ps1 -TrailId`) but that folder already exists. Practically unreachable when the id is a freshly-minted guid; a safety check, not a primary path. |
| `trail-sealed` | The referenced trail's `disposition.json` has `sealed: true`. |
| `trail-linked` | The referenced trail's `disposition.json` shows `disposition` already bound to another active cycle. |
| `io-failure` | Folder/file creation failed (permissions, disk, lock). |

## Refusal steps

1. Do not create, modify, or delete anything.
2. Identify the single matching reject code above (first match wins, in the
   table's order).
3. Return the refusal result shape below. Never fall through to the accept
   path on a partial precondition failure.

## Refusal result

```json
{
  "status": "rejected",
  "reject_code": "<one of the codes above>",
  "message": "<human-readable reason, no drive-letter or host-specific paths>"
}
```

## Non-goals

- Does not retry or self-heal (e.g. does not un-seal a sealed trail).
- Does not propose a next seed — that is Reflector's job, on a later,
  separate cycle.
