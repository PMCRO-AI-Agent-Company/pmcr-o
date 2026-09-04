# .agents — maintainer tooling

This directory is **not** shipped product content. It mirrors the shape used by
[dotnet/skills](https://github.com/dotnet/skills/tree/main/.agents) and by
this colony's own [pmcro-skills_archive](https://github.com/PMCRO-AI-Agent-Company/pmcro-skills_archive)
repo: local authoring helpers for people working *on* this repository, not
PMCR-O lifecycle mechanics themselves (those stay under `plugins/pmcro-*/`).

| Path | Purpose |
|------|---------|
| `skills/create-skill/` | Scaffold a new skill under the right `plugins/pmcro-*/skills/` |
| `skills/create-skill-test/` | Author `eval.yaml` for a skill (baseline-vs-skilled trials) |
| `skills/improve-skill-quality/` | Fix a skill that fails its eval or loses to baseline |

Runtime / installable product lives only under `plugins/pmcro-*/`.

Do **not** put laws, trails, identity injection, or seed-queue here — those are
`.pmcro/` runtime concerns, not this authoring surface.

## Source and adaptation

Ported from [pmcro-skills_archive](https://github.com/PMCRO-AI-Agent-Company/pmcro-skills_archive)
`.agents/` @ `main` (commit `d864f70`), at the maintainer's direction, then
trimmed to exactly this shape — `agents/`, `commands/`, `memory/`, and
`skills/export-source/` were all explicitly removed, not just deferred.
Kept content was adapted rather than left as a false claim about this repo:

- `skills/create-skill/SKILL.md`'s "Plugin targets" list was rewritten to
  this repo's actual six plugins (the archive's own list had drifted to
  names — `pmcro-system`, `pmcro-strategy`, `pmcro-actuator` — that don't
  exist here).
- The trailing "PMCRO Output Law" section and `maf: native_skill: true`
  frontmatter field were dropped from every ported `SKILL.md`. This repo
  has no `.pmcro/runtime/output-contract.md`, no `L-OUTPUT-CONTRACT`, and
  no Microsoft Agent Framework (MAF) runtime — keeping those lines would
  have asserted infrastructure that isn't real here. If this repo adopts
  an output contract or MAF later, restore them deliberately, not as a
  side effect of a mirror.
- `skills/create-skill/SKILL.md` gained one line not in the archive:
  point at the `command.<name>.asset.md` / `run.<name>.asset.md` /
  `reject.<name>.asset.md` triad already established across every
  `plugins/pmcro-*/skills/*/` in this repo, so a newly scaffolded skill
  follows this repo's own real convention, not just the archive's.

Removed on top of that, at the maintainer's explicit direction (matching
the same trim made in the archive's own redesign session): `README.md`'s
`plugins/marketplace.json` mirror (this repo keeps one canonical
`.claude-plugin/marketplace.json`, nothing to mirror or sync-check),
`agents/skill-quality-reviewer.agent.md`, `commands/` (both
`agents-sync-check.md` and `export-source.md`), `memory/` (on-disk Claude
memory mirror), and `skills/export-source/` in full (SKILL.md, eval.yaml,
assets/, references/, scripts/export_source.py) — the script was tested
for real against this repo before this removal decision and worked
correctly, but the whole capability was cut regardless, per direction.
