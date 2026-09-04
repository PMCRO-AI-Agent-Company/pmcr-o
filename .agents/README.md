# .agents — maintainer tooling

This directory is **not** shipped product content. It contains local authoring
and repository-maintenance helpers for people working *on* this repository.
PMCR-O lifecycle mechanics remain under `plugins/pmcro-*/`, while durable
governance and evidence remain under `.pmcro/`.

| Path | Purpose |
|------|---------|
| `skills/create-skill/` | Scaffold a new skill under an existing PMCRO plugin |
| `skills/create-skill-test/` | Author `eval.yaml` using baseline-vs-skilled trials |
| `skills/improve-skill-quality/` | Diagnose and fix failed skill evaluations |
| `export-source-dump.ps1` | Deterministically export a root-relative source-file inventory |

## Export source dump

`export-source-dump.ps1` writes a sorted, root-relative file inventory to an
explicit output file. It accepts exclusion patterns, rejects drive-root
outputs, creates a missing destination directory, and excludes the output
file itself when it is inside the source tree.

Example:

```powershell
& .agents/export-source-dump.ps1 `
  -Root . `
  -Exclude '.git','.vs','*.zip','.pmcro/trails' `
  -OutputPath .artifacts/source-dump.txt
```

The dump is an inventory, not a copy of file contents. It is intended for
architecture audits, source snapshots, and reproducible repository inspection.

## Maintainer skill rules

- Keep authoring helpers separate from shipped plugin content.
- Use the six existing production plugins; do not create a plugin merely to
  hold a capability already provided elsewhere.
- Production skills use `SKILL.md`, optional `assets/`, `references/`, and
  `scripts/`, plus required `eval.yaml` governance metadata.
- Never put runtime state, trails, queues, laws, secrets, or custom loaders in
  a production skill package.
- Keep evaluation thresholds explicit and never lower them to obtain a pass.
- Prefer deterministic scripts for filesystem/package operations and verify
  their behavior against real repository paths before claiming success.

## Source and adaptation

The maintainer skills were adapted from the repository's historical
`pmcro-skills_archive` authoring surface, then aligned to this repository's
actual six-plugin topology and active `.pmcro/` governance layer. Historical
archive-only paths are not treated as live repository capabilities unless they
are deliberately restored and validated here.
