---
name: initialize
description: Materializes a durable PMCR-O trail (Class B guid-folder, one jsonl per phase + disposition.json). USE FOR — allocating or linking the evidence store a cycle writes its frames into. DO NOT USE FOR — deciding whether to open a cycle (that is pmcro-orchestrator's orchestrate skill), or reasoning about cycle content.
metadata:
  version: "0.1.0"
  tier: GOVERNANCE
  capability_class: EVIDENCE
---

# Initialize

## Command

`/pmcro-trail:initialize` — see `assets/command.initialize.asset.md` for the
full contract (invocation, parameters, result/error shape). This section is
a pointer, not a duplicate — the asset file is the single source of truth.

## Purpose

The durable capability that materializes and owns the trail-as-product
shape: a guid-named folder under `.pmcro/trails/` containing one JSONL file
per phase (`plan.jsonl`, `make.jsonl`, `check.jsonl`, `reflect.jsonl`) plus a
`disposition.json` manifest. Every line appended to a phase file is an
instance of `assets/schema.trail-frame.asset.json` — frames are declared and
validated, never hand-typed prose.

## Inputs / Outputs / Boundaries

See `assets/command.initialize.asset.md`, `assets/run.initialize.asset.md`,
and `assets/reject.initialize.asset.md`.

## Workflow (for anyone scaffolding a command on this skill)

Copy `assets/schema.trail-frame.asset.json` as the frame contract; do not
invent new frame fields ad hoc. Copy `command.*` / `run.*` / `reject.*`
patterns from this skill when authoring a new one. Do not retype.

## References

- `assets/command.initialize.asset.md`
- `assets/run.initialize.asset.md`
- `assets/reject.initialize.asset.md`
- `assets/schema.trail-frame.asset.json`
- `references/README.md`
