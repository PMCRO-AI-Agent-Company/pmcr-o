---
asset_type: pmcro-command-contract
version: "0.1.0"
target_skill: reflect-and-seed
---

# Command Shape: reflect-and-seed run

## Purpose

Close a cycle: record a Reflection with a disposition, optionally file a
next seed, then seal the trail.

## Invocation

```text
/pmcro-reflector:reflect-and-seed run --trail-id <guid>
```

## Parameters

| Name | Required | Default | Meaning |
|---|---|---|---|
| `--trail-id` | yes | — | The trail being closed. Must exist, be unsealed, and have a CheckFrame verdict to reflect on. |

The Reflection's own content (`content`, `disposition`, `next_seed`) and
any queue item's content are composed by Reflector per
`schema.trail-frame.asset.json` and `schema.queue-item.asset.json`
respectively — not CLI flags.

## Result (success)

```json
{
  "status": "ok",
  "trail_id": "<guid>",
  "phase": "reflect",
  "seq": 1,
  "disposition": "done",
  "sealed": true,
  "next_seed_enqueued": false
}
```

## Result (failure)

See `reject.reflect-and-seed.asset.md` for refusal shapes and reject codes.

## Related Assets

- `run.reflect-and-seed.asset.md` — accept-path implementation
- `reject.reflect-and-seed.asset.md` — refusal-path implementation
- `schema.queue-item.asset.json` — shape of any filed queue item
- `pmcro-checker`'s `run.check.asset.md` — the predecessor step
- `pmcro-orchestrator`'s `command.orchestrate.asset.md` — where a filed
  seed eventually lands, on a later, separate cycle
