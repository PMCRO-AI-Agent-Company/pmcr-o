---
name: run-pmcro-cycle
description: Use when opening, working, or closing a PMCR-O cycle in this repo — i.e. whenever a fix, feature, or investigation should be logged as a governed, schema-conformant trail rather than done ad hoc. Encodes the exact mint-log-seal-commit procedure this repo's own build history follows.
---

# Running a logged PMCR-O cycle in this repo

Trail-as-Product means every piece of real work in this repo — not just
demos of the mechanism — gets logged as a Class B trail: five phase
files (`orchestrate.jsonl`, `plan.jsonl`, `make.jsonl`, `check.jsonl`,
`reflect.jsonl`) plus `disposition.json`, each line an instance of
`plugins/pmcro-trail/skills/initialize/assets/schema.trail-frame.asset.json`.
This skill is the procedure, not the schema — read the schema itself for
field shapes.

## The five roles now exist as real plugins

Earlier in this repo's history, Planner/Maker/Checker/Reflector were
hand-simulated — one agent writing all five files itself, in character.
That's no longer necessary: each role is a real plugin with its own
command.

| Phase | Plugin : skill | Writes to | Ends by |
|---|---|---|---|
| Open | `pmcro-orchestrator:orchestrate run` | `orchestrate.jsonl` | handing off to Planner |
| Plan | `pmcro-planner:plan run` | `plan.jsonl` | handing off to Maker |
| Make | `pmcro-maker:make step` (once per step) | `make.jsonl` | handing off to itself (next step) or Checker |
| Check | `pmcro-checker:check run` | `check.jsonl` | handing off to Reflector |
| Reflect | `pmcro-reflector:reflect-and-seed run` | `reflect.jsonl` | sealing the trail |

Whether "running" a command here means literally invoking it or an agent
following its `run.<name>.asset.md` instructions and writing the file
directly is an implementation detail of whatever is executing the cycle —
the contract (preconditions, file ownership, result/reject shapes) is the
same either way. Every one of these plugins' own `command`/`run`/`reject`
asset triads is the actual source of truth; this skill only sequences them.

## The procedure

1. **Open.** Mint (or link) a trail — `pmcro-orchestrator:orchestrate run`,
   or directly via `plugins/pmcro-trail/skills/initialize/scripts/New-Trail.ps1
   -PmcroRoot ".\.pmcro" -Class B` for the mint path. Use `-OutputPath` only
   for scratch/testing; real work mints under the repo's own `.pmcro/trails/`.
2. **Log incrementally, not as an end-of-cycle dump.** Write each phase's
   frame(s) as that work actually happens — a MakeStep right after the
   step it describes, not five files authored in one pass at the end from
   memory. This repo's whole build history is evidence a session can do
   this; don't regress to narrating in chat and writing the trail last.
3. **Plan → Make → Check → Reflect**, each via its own plugin, each
   appending only to the file it owns.
4. **Seal via Reflector, not by hand-editing `disposition.json` elsewhere.**
   `reflect-and-seed` is the only place `sealed: true` should be set.
5. **Commit in the same session as the cycle**, with the trail id cited in
   the commit message, and push. A sealed trail with no matching commit is
   a broken handoff for whoever reads this repo's history next.

## Hard rules carried over from every role's own agent file

- No absolute, host-specific, or drive-letter paths in anything any role
  writes.
- Every phase file has exactly one owning role; nothing else appends to it.
- A sealed trail is immutable — a mistake found later gets a *new* trail
  and, if warranted, a `next_seed` referencing the old one, never an edit
  to sealed history.

## Related

- `.claude/skills/create-plugin/` — the sibling skill for packaging a
  *plugin* correctly; this skill is about running a *cycle* correctly.
  They're both repo-authoring tooling, not colony products.
