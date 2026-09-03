---
asset_type: pmcro-command-contract
version: "0.1.0"
target_skill: initialize
target_schema: schema.trail-frame.asset.json
---

# Command Shape: initialize

## Purpose

Materialize (mint) or link (verify + bind) a Class B trail folder under
`.pmcro/trails/<guid>/` so a cycle has somewhere to write its frames — one
jsonl file per role (`orchestrate.jsonl`, `plan.jsonl`, `make.jsonl`,
`check.jsonl`, `reflect.jsonl`), each written only by the role that owns it.

## Invocation

```text
/pmcro-trail:initialize [--trail-id <guid>] [--class A|B] [--output-path <path>]
```

Also callable as an internal step from `pmcro-orchestrator:orchestrate run`'s
accept path (see that skill's `run.orchestrate.asset.md`) — same contract,
no marketplace round-trip required.

## Parameters

| Name | Required | Default | Meaning |
|---|---|---|---|
| `--trail-id` | no | mint a new guid | If supplied, link to an existing trail folder instead of minting one. |
| `--class` | no | `B` | `B` = guid-folder + per-phase jsonl (default, this repo's only implemented shape). `A` = single markdown file — reserved, not implemented by this skill yet. |
| `--output-path` | no | `.pmcro/trails/<trail_id>/` | Override the target folder entirely. Production cycles should omit this and take the default. Intended for scratch/testing runs — exercising this skill without writing into live evidence. The resulting `disposition.json` records `scratch: true` when this is used, and the trail's own `path` field echoes whatever was supplied — callers must not pass a drive-letter or other host-specific path here if that value will later be logged into a frame. |

## Result (success)

```json
{
  "status": "ok",
  "trail_id": "<guid>",
  "trail_class": "B",
  "path": "trails/<guid>/",
  "files": ["orchestrate.jsonl", "plan.jsonl", "make.jsonl", "check.jsonl", "reflect.jsonl", "disposition.json"]
}
```

## Result (failure)

See `reject.initialize.asset.md` for the refusal shapes and their reject
codes (`trail-not-found`, `trail-sealed`, `trail-linked`, `class-a-unsupported`,
`io-failure`).

## Related Assets

- `run.initialize.asset.md` — accept-path implementation
- `reject.initialize.asset.md` — refusal-path implementation
- `schema.trail-frame.asset.json` — the frame contract every jsonl line must satisfy
