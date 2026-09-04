# Evidence

Evidence is the proof layer for PMCR-O claims and completion.

- `executions/` — commands, tool calls, and execution records
- `checks/` — Checker findings and validation results
- `artifacts/` — evidence-backed output references
- `provenance/` — source, version, and lineage metadata

A completion claim without evidence is not a valid PMCR-O completion.

## Source and adaptation

Ported verbatim from `pmcro-skills_archive` `.pmcro/evidence/README.md` @
`main` (commit `d864f70`) — genuinely portable, and directly consistent
with `L-EVIDENCE`/`L-CHECKER-GATE` in `../laws/` and this repo's own
existing Check-phase discipline. None of the four subdirectories exist
yet: this repo's actual evidence today lives inline, as part of each
sealed trail's `make.jsonl`/`check.jsonl` phase files, rather than as
separate referenced artifacts under a dedicated `evidence/` tree. A
dedicated `evidence/` layer becomes worth populating once evidence
artifacts (logs, screenshots, test output) need to be referenced from
outside their originating trail, or reused across trails.
