# Harness (runtime notes)

A harness, in the archive's usage, is what would run cycles autonomously
— claiming queue items, dispatching roles, and looping — without a human
driving each phase by hand.

## Source and adaptation

Ported and adapted from `pmcro-skills_archive`
`.pmcro/runtime/harness/HARNESS.md` @ `main` (commit `d864f70`). The
archive's version references "Maker CodeAct" (a Microsoft Agent
Framework execution mode) and a set of "ROUND-TABLE" design docs that
don't exist in this repo — both dropped.

This repo has no harness today. Every cycle so far — including the ones
that produced this `.pmcro/` mirror — has been run by a human or agent
manually following each role's `run.<name>.asset.md` in turn:
`plugins/pmcro-orchestrator` → `plugins/pmcro-planner` →
`plugins/pmcro-maker` (repeated per step) → `plugins/pmcro-checker` →
`plugins/pmcro-reflector`. `.claude/skills/run-pmcro-cycle` documents
that exact procedure. A future harness would be the thing that automates
this sequence; it isn't invented here just because the archive has one.
