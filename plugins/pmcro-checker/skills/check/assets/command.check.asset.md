---
asset_type: pmcro-command-contract
version: "0.1.0"
target_skill: check
---

# Command Shape: check run

## Purpose

Evaluate the trail's PlanFrame `success_criteria` against its logged
MakeSteps and append one CheckFrame with a verdict.

## Invocation

```text
/pmcro-checker:check run --trail-id <guid>
```

## Parameters

| Name | Required | Default | Meaning |
|---|---|---|---|
| `--trail-id` | yes | — | The trail whose Make phase is being verified. Must exist, be unsealed, have a PlanFrame, and have at least one MakeStep. |

The CheckFrame's own content (`criteria[]`, `verdict`) is composed by
Checker per `schema.trail-frame.asset.json` after actually evaluating the
evidence — not a CLI flag.

## Result (success)

```json
{
  "status": "ok",
  "trail_id": "<guid>",
  "phase": "check",
  "seq": 1,
  "verdict": "PASS"
}
```

`verdict: "FAIL"` is still a successful *command* invocation — the
evaluation happened and was logged honestly. See `reject.check.asset.md`
only for cases where no evaluation could be logged at all.

## Result (failure)

See `reject.check.asset.md` for refusal shapes and reject codes.

## Related Assets

- `run.check.asset.md` — accept-path implementation
- `reject.check.asset.md` — refusal-path implementation
- `pmcro-maker`'s `run.make.asset.md` — the predecessor step
- `pmcro-reflector`'s `command.reflect-and-seed.asset.md` — the successor step
