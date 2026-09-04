# config

Runtime parameters (`parameters.yaml`) and, in the archive, an
environment map (`environments.yaml`) for Development/Test/Production.

## Source and adaptation

Ported from `pmcro-skills_archive` `.pmcro/config/` @ `main` (commit
`d864f70`). `parameters.yaml` in the archive lists Aspire/Docker/Azure
concrete parameters (`github-token`, `docker-mcp-profile`,
`azure-key-vault-uri`, ...); none apply here, so the list is empty with a
comment, keeping only the generic rules (never commit secret values,
resolve from a real secret provider in production).

`environments.yaml` was **not ported**. This repo has one working copy
and one deployment target — itself, as a Cowork-installed plugin set —
not a Development/Test/Production split with per-environment Docker MCP
profiles and Azure Key Vault sourcing. The archive's own live copy of
this file, read during this mirror, already carried a maintainer note
that its own Dev/Test/Prod entries shared one underlying profile with
"no real isolation between them yet" — porting a three-environment map
into a repo with zero environments would have been a bigger fiction than
the one it was already flagging in its source. Add `environments.yaml`
if and when this repo actually has more than one deployment target.
