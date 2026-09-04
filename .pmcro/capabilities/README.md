# capabilities

A capability is a stable name for a *kind* of thing the colony can do
(`browser`, `filesystem`, `host-command-execution`, ...) — independent of
which tool or MCP server actually implements it. `registry.yaml` is the
index; `providers/` says who implements each one; `mcp/` is the
MCP-specific slice of that routing.

## Source and adaptation

Ported from `pmcro-skills_archive` `.pmcro/capabilities/registry.yaml` @
`main` (commit `d864f70`). The archive lists nine capabilities, each with
its own leaf YAML (`browser.yaml`, `containers.yaml`, `evidence.yaml`,
`filesystem.yaml`, `github.yaml`, `host-command-execution.yaml`,
`lifecycle.yaml`, `mcp-gateway.yaml`, `memory.yaml`). None of those leaf
files were ported: this repo has no provider behind any of them yet (see
`../providers/README.md`), and writing a capability definition ahead of a
real provider would be exactly the "invented integration" the registry's
own rules warn against. `registry.yaml`'s `capabilities` map is
intentionally empty here, with a comment pointing at what to do instead
of a fabricated list.

Add a capability when it's real: a provider exists (`../providers/`), a
policy decision is made for it (`../policies/execution.yaml`), and it's
actually invoked by some skill in `plugins/pmcro-*/`.
