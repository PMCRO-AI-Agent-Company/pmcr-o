---
name: create-plugin
description: Use when scaffolding a new plugin under plugins/, or when packaging/re-packaging an existing plugin (plugin.json changes, agent files added, or building a .zip for Cowork's "Upload a plugin" flow). Encodes every plugin-upload failure this repo has actually hit, so they aren't rediscovered one at a time.
---

# Creating and packaging a plugin in this repo

## Purpose

The repo-authoring capability (not a colony/PMCR-O product) that scaffolds
a new plugin correctly and packages any plugin into a `.zip` Cowork will
actually accept — every rule it encodes came from a real failed upload or
a real failed `claude plugin validate` call in this repo, not from reading
Cowork's docs ahead of time.

## Workflow

1. Read `assets/packaging-checklist.asset.md` in full before scaffolding or
   repackaging — it is the single source of truth, not duplicated here.
2. Build the zip with `scripts/New-PluginZip.ps1` (never
   `Compress-Archive`, never a one-off script) — see §3 of the checklist.
3. Verify the built zip and, if `claude` is available, run
   `claude plugin validate <plugin-dir>` — see §4.
4. If this was a real fix (not fresh scaffolding), log it as a sealed
   trail per this repo's own PMCR-O discipline — see §5, and
   `run-pmcro-cycle` for the full mint→log→seal→commit procedure.

## References

- `assets/packaging-checklist.asset.md` — the full checklist
- `scripts/New-PluginZip.ps1` — deterministic zip-build implementation;
  call this rather than hand-rolling a zip
- `references/README.md`
