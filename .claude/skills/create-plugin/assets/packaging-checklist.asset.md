---
asset_type: pmcro-repo-checklist
version: "0.1.0"
target_skill: create-plugin
---

# Packaging checklist: scaffolding or (re)packaging a plugin

This repo's plugins (`plugins/pmcro-trail`, `plugins/pmcro-orchestrator`, and
any future ones) get uploaded to Cowork as `.zip` archives. Cowork's upload
validator is stricter than `claude plugin validate`, and every rule below
was found by an actual failed upload, not by reading docs. Follow this
checklist before telling a human "it should work now."

## 1. Manifest location and shape

- The manifest MUST exist at `.claude-plugin/plugin.json` (not just a plain
  root-level `plugin.json`). Cowork's uploader specifically looks there.
  This repo keeps both a root `plugin.json` (for `claude plugin validate`
  / CLI installs) and an identical `.claude-plugin/plugin.json` — keep them
  in sync by hand; there's no build step that copies one to the other.
- Required/recommended fields: `name`, `version`, `description`,
  `author: { "name": "..." }`, `skills`, `agents` (see §2), `keywords`.
- `skills` is a directory reference (e.g. `"./skills/"`), plural components
  live under it.
- `agents` is an array of **literal `.md` file paths**, never a directory
  (`["./agents/orchestrator.md"]`, not `["./agents/"]`). A directory path
  here silently breaks org-level Cowork plugin sync without any upload-time
  error — that's how the legacy repo's manifest broke.

## 2. Agent filenames: no dots before `.md`

Cowork derives an agent's display name from its file's basename (the
filename minus the trailing `.md`), and that name must start/end
alphanumeric and contain only letters, numbers, and hyphens.

- **Wrong:** `agents/orchestrator.agent.md` → derived name `orchestrator.agent`
  → rejected: "Agent name '...' must start and end with alphanumeric
  characters and contain only letters, numbers, and hyphens."
- **Right:** `agents/orchestrator.md` (bare `<role>.md`, no extra dots).
- Belt-and-suspenders: also set an explicit `name:` field in the agent's
  own frontmatter, in case a future validator prefers that over filename
  derivation.
- This constraint is specific to **agent** files. The skill-asset naming
  convention used elsewhere in this repo — `command.<name>.asset.md`,
  `run.<name>.asset.md`, `reject.<name>.asset.md` — is unaffected; those
  aren't agent files and Cowork doesn't derive a name from them.

## 3. Building the `.zip`: never use `Compress-Archive`

On Windows, PowerShell's `Compress-Archive` writes internal zip entry paths
with backslashes (e.g. `.claude-plugin\plugin.json`). Backslash is not a
valid zip path separator per spec, and Cowork's validator rejects the whole
archive with "Zip file contains path with invalid characters" — even
though every file inside is otherwise correct.

Always build plugin zips with this skill's own `scripts/New-PluginZip.ps1`
instead — run it from the repo root:

```powershell
.\.claude\skills\create-plugin\scripts\New-PluginZip.ps1 -PluginDir .\plugins\<plugin-name> -OutFile .\<plugin-name>.zip
```

That script builds the archive via `System.IO.Compression` directly and
normalizes every entry to forward slashes. It also resolves `-OutFile`
through PowerShell's own path provider before handing it to .NET — a
relative path passed straight to `[System.IO.Compression.ZipFile]::Open()`
resolves against the *process's* real working directory, not wherever
`Set-Location` last pointed PowerShell, and silently writes (or fails to
write) somewhere unexpected. If you ever reimplement zip-building instead
of reusing the script, keep that resolution step.

## 4. Verify before telling anyone to upload

After building, reopen the zip and print its entries — don't assume the
script worked:

```powershell
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path .\<plugin-name>.zip))
$zip.Entries | ForEach-Object { $_.FullName }
$zip.Dispose()
```

Confirm: every path uses `/`, `.claude-plugin/plugin.json` is present at
the archive root, and the byte size / file count look non-trivial (a
0-byte or near-empty zip usually means the working-directory gotcha in
§3 bit you again).

If the `claude` CLI is available, also run `claude plugin validate
<plugin-dir>` first — it catches manifest-shape issues (missing
`description` in agent frontmatter, malformed `agents` entries) that are
cheaper to fix before even building the zip. Note this takes the plugin's
*directory*, not the zip — `claude plugin validate <zip-path>` fails with
a confusing "Invalid JSON syntax" error because it tries to parse the zip
bytes themselves as JSON.

## 5. Log the fix

Per this repo's own PMCR-O discipline: if you're fixing a real upload
failure (not just scaffolding fresh), open and seal a trail for it via
`plugins/pmcro-trail/skills/initialize/scripts/New-Trail.ps1` (and the
matching orchestrate/plan/make/check/reflect scripts for the rest of the
cycle — see `run-pmcro-cycle`), same as any other cycle in this repo.
Don't let a real, reusable finding live only in chat history.

## Known-good reference

`plugins/pmcro-orchestrator/`, `plugins/pmcro-trail/`, `plugins/pmcro-planner/`,
`plugins/pmcro-maker/`, `plugins/pmcro-checker/`, and `plugins/pmcro-reflector/`
are all current, working examples of every rule above — diff against
whichever is most similar in shape if something is unclear.
