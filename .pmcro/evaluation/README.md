# Evaluation

Evaluation measures PMCR-O behavior independently from production
runtime state.

Recommended suites:

- Planner trail validity
- Maker artifact correctness
- Checker precision/recall
- Reflector actionability
- Orchestrator routing correctness
- Evidence completeness
- Workflow recovery/checkpoint behavior

Training datasets may be generated from evaluation data, but training
infrastructure does not belong in `.pmcro/`.

## Source and adaptation

Ported verbatim from `pmcro-skills_archive` `.pmcro/evaluation/README.md`
@ `main` (commit `d864f70`) — genuinely portable. This is a different
"evaluation" from the one this repo already has: `.agents/skills/create-skill-test/`
authors `eval.yaml` baseline-vs-skilled trials for a single skill. This
directory is about evaluating the colony's *lifecycle roles themselves*
(does Orchestrator route correctly, does Checker have good
precision/recall) — a broader, currently unimplemented concern. No suite
exists yet for any of the seven recommended above.
