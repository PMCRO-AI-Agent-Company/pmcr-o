---
name: reflect-and-seed
description: Closes a cycle — records a Reflection with a disposition, optionally proposes and enqueues a next seed, then seals the trail. USE FOR — deciding a cycle is finished and what, if anything, follows. DO NOT USE FOR — planning, making, checking, or opening the next cycle itself.
metadata:
  version: "0.1.0"
  tier: GOVERNANCE
  capability_class: PHASE
---

# Reflect-and-seed

## Command

`/pmcro-reflector:reflect-and-seed run` — see
`assets/command.reflect-and-seed.asset.md` for the full contract. This
section is a pointer, not a duplicate.

## Purpose

Reads the trail's CheckFrame verdict, appends exactly one Reflection to
`reflect.jsonl` recording a `disposition` and an optional `next_seed`,
files that seed into `.pmcro/queue/` when present, then seals the trail.
This is the only skill in the colony permitted to seal.

## Inputs / Outputs / Boundaries

See `assets/command.reflect-and-seed.asset.md`,
`assets/run.reflect-and-seed.asset.md`, and
`assets/reject.reflect-and-seed.asset.md`.

## References

- `assets/command.reflect-and-seed.asset.md`
- `assets/run.reflect-and-seed.asset.md`
- `assets/reject.reflect-and-seed.asset.md`
- `assets/schema.queue-item.asset.json`
- `scripts/Complete-ReflectAndSeed.ps1` — the deterministic implementation
  of the accept path; call this rather than writing `reflect.jsonl`,
  `.pmcro/queue/`, or `disposition.json` directly
- `references/README.md`
