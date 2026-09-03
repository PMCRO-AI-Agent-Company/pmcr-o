# pmcro-skills (fresh)

A clean, from-scratch PMCR-O colony repo. Git-initialized with no history or
content carried over from any other checkout on this host — this repo is
entirely self-contained.

## What's here so far

- `plugins/pmcro-trail/` — durable trail/evidence capability (`initialize`
  skill, `New-Trail` script, Class B guid-folder trail shape, and the
  Trail-as-Product frame schema every jsonl line conforms to).
- `plugins/pmcro-orchestrator/` — sole dispatch authority (`orchestrate`
  skill), convenient mint-or-link trail-id contract.
- `.pmcro/trails/` — sealed cycle evidence for this repo's own build history
  (dogfooded: this repo's own scaffolding is itself being run as a PMCR-O
  cycle, Class B, one frame file per phase, each line schema-conformant).
- `.pmcro/queue/` — seed-intent inbox (currently empty).

## Conventions

- No absolute, host-specific, or drive-letter paths in system-authored
  content (frames, docs, configs). Reference files repo-relative.
- Trail-as-Product: trail frames are instances of a declared schema
  (`plugins/pmcro-trail/skills/initialize/assets/schema.trail-frame.asset.json`),
  not hand-typed prose.

## Not here yet (intentionally out of scope for the first cycle)

Planner / Maker / Checker / Reflector role plugins, a `.agents/` authoring
surface, laws/policies/capabilities layers. These land as their own seeds,
not invented ahead of being asked for.
