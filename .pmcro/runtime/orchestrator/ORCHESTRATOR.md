# Orchestrator (runtime notes)

The Orchestrator is the colony's sole dispatch authority: it opens a
cycle, routes work to Planner/Maker/Checker/Reflector in sequence, and
never implements domain logic itself (`L-ORCHESTRATION`).

## Source and adaptation

Ported and adapted from `pmcro-skills_archive`
`.pmcro/runtime/orchestrator/ORCHESTRATOR.md` @ `main` (commit `d864f70`).
The archive's version describes routing against a `trail.json` skeleton
and a "Harness mode" for autonomous multi-cycle runs. This repo's real
Orchestrator (`plugins/pmcro-orchestrator/skills/orchestrate/`) doesn't
match either of those shapes: there is no single `trail.json` — a trail
is a guid-named folder with one jsonl file per phase
(`orchestrate.jsonl`, `plan.jsonl`, `make.jsonl`, `check.jsonl`,
`reflect.jsonl`), and there is no harness/autonomous-run mode — a human
or agent runs one cycle at a time by following
`plugins/pmcro-orchestrator/skills/orchestrate/assets/run.orchestrate.asset.md`.

What's kept here is the role description itself (sole dispatch authority,
no domain implementation), which is accurate for this repo and already
matches `plugins/pmcro-orchestrator`'s own `SKILL.md`. This file exists
in `.pmcro/runtime/` as a mirror of the archive's shape, not as the
authoritative description of Orchestrator's mechanics — that's
`plugins/pmcro-orchestrator/skills/orchestrate/SKILL.md` and its asset
triad. If the two ever disagree, the plugin skill is correct.
