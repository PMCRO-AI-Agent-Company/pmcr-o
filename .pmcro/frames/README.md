# Frames

A Frame is the normalized representation of user intent, context,
constraints, desired outcome, and relevant evidence references.

Frames are inputs to Trails and workflows. They are not agent memory and
should remain immutable once an execution begins; derived updates belong
to state or a new Frame revision.

## Source and adaptation

Ported verbatim from `pmcro-skills_archive` `.pmcro/frames/README.md` @
`main` (commit `d864f70`) — genuinely portable.

One naming note, so this doesn't read as contradicting an existing
convention: this repo already uses the word "frame" for something
related but narrower — each schema-conformant jsonl line a role plugin
appends to a trail's `orchestrate.jsonl` / `plan.jsonl` / `make.jsonl` /
`check.jsonl` / `reflect.jsonl` (see
`plugins/pmcro-trail/skills/initialize/assets/schema.trail-frame.asset.json`).
The archive's "Frame" here is the *input* to a cycle — normalized intent,
before a trail exists — which this repo currently just calls "the task"
in a queue item or human message. Both usages are legitimate PMCR-O
vocabulary and aren't actually in conflict (input frame vs. per-phase
trail frame), but a future normalized-intent-frame implementation in this
repo should pick a name that doesn't collide with the existing trail-frame
schema, or explicitly document the two as related-but-distinct.
