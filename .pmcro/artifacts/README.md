# Artifacts

Cycle outputs are separated from execution state and evidence.

- `plans/` — Planner outputs
- `outputs/` — Maker outputs
- `checks/` — Checker outputs
- `reflections/` — Reflector outputs

Artifacts should reference their originating Frame, Trail, run, and
evidence where applicable.

## Source and adaptation

Ported verbatim from `pmcro-skills_archive` `.pmcro/artifacts/README.md`
@ `main` (commit `d864f70`) — genuinely portable, and the four
subdirectories map directly onto this repo's own four downstream roles
(Planner/Maker/Checker/Reflector). None exist yet: this repo's actual
per-role outputs currently live inline in each trail's own phase files
(`plan.jsonl`, `make.jsonl`, `check.jsonl`, `reflect.jsonl`) rather than
as separately filed artifacts. A dedicated `artifacts/` layer becomes
worth populating once an output needs to be referenced or reused outside
its originating trail (a generated file, a build output, a report) rather
than living entirely inside the trail's own jsonl record.
