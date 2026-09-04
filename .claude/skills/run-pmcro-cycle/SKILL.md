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

| Phase | Plugin : skill | Deterministic script | Writes to | Ends by |
|---|---|---|---|---|
| Open | `pmcro-orchestrator:orchestrate run` | `scripts/New-OrchestrateFrame.ps1` (after minting via `pmcro-trail`'s `New-Trail.ps1`, or directly for the link path) | `orchestrate.jsonl` | handing off to Planner |
| Plan | `pmcro-planner:plan run` | `scripts/New-PlanFrame.ps1` | `plan.jsonl` | handing off to Maker |
| Make | `pmcro-maker:make step` (once per step) | `scripts/New-MakeStep.ps1` | `make.jsonl` | handing off to itself (next step) or Checker |
| Check | `pmcro-checker:check run` | `scripts/New-CheckFrame.ps1` | `check.jsonl` | handing off to Reflector |
| Reflect | `pmcro-reflector:reflect-and-seed run` | `scripts/Complete-ReflectAndSeed.ps1` | `reflect.jsonl` | sealing the trail |

**"Running" a command means calling its script, not hand-writing the
phase file.** Every role's `run.<name>.asset.md` describes what the
caller reasons about (the frame's substantive content: a goal, what
happened, a verdict, a disposition) — the actual file mechanics
(precondition checks, computing the next `seq`, producing correctly
schema-shaped JSON, appending) are a deterministic, zero-reasoning
PowerShell script, mirroring `pmcro-trail`'s own `New-Trail.ps1`. This
matters beyond tidiness: this exact split caught real bugs — Windows
PowerShell 5.1's `ConvertTo-Json` silently corrupts array-valued fields
written by hand or built carelessly, and PowerShell's `Set-Location`
doesn't move the process CWD that raw file APIs resolve against. A script
that's actually run and tested surfaces those; hand-composed JSON typed
fresh each cycle does not, and re-introduces the exact hand-typed-frame
problem Trail-as-Product exists to prevent.

All five phases now have one: Orchestrator's own claim/link step is
`scripts/New-OrchestrateFrame.ps1` — it doesn't mint or link a trail
itself (minting is still `pmcro-trail`'s `New-Trail.ps1`), but its own
precondition checks (exists, unsealed, not already claimed) *are* the
link-path verification for a supplied `--trail-id`, so claiming and
verifying are one deterministic call, not two.

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
