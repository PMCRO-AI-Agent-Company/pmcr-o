# providers

Who actually implements a capability — a concrete tool, MCP server, or
runtime-native mechanism. `registry.yaml` is the index; `../mcp/` is the
MCP-specific slice of the same routing.

## Source and adaptation

Ported from `pmcro-skills_archive` `.pmcro/providers/registry.yaml` @
`main` (commit `d864f70`). The archive's registry names six concrete
providers: `github` (GitHub MCP), `playwright` (browser MCP),
`docker-mcp-toolkit` (MCP gateway/containers), `terminal` (native host
commands), `maf-native` (Microsoft Agent Framework native filesystem
tooling), and `pmcro-runtime` (a second, separate repo in this colony
providing memory/evidence/evaluation). None of these exist in this repo —
no MAF runtime, no Docker MCP Toolkit, no `pmcro-runtime` repo, and
GitHub/browser access here goes through this session's own tools, not a
registered MCP provider this registry would route to.

`registry.yaml`'s `providers` map is intentionally empty, with a comment
explaining why, rather than either copying entries that would misdescribe
this repo or inventing different placeholder entries — both would be a
claim about infrastructure that isn't real here. The generic rules
(credentials never live in this file, missing-provider escalates rather
than invents) were kept — those are true regardless of which providers
exist.

Add a provider entry when this repo actually registers one — for example
if this session's device-bridge tools, or a specific MCP server, become a
formally routed capability provider rather than an ambient session tool.
