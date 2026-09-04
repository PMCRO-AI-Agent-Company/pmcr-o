# mcp

The MCP-specific slice of provider routing — which MCP server backs which
capability, and the protocol version this colony targets. Overlaps with
`../providers/` by design (the archive keeps them as separate registries:
one generic, one MCP-specific); kept as two files here for the same
reason, even though both are currently empty.

## Source and adaptation

Ported from `pmcro-skills_archive` `.pmcro/mcp/registry.yaml` @ `main`
(commit `d864f70`). The archive pins a protocol date and routes four
capabilities to four MCP providers (GitHub MCP, Playwright MCP, Docker
MCP Toolkit, and native terminal). Neither the protocol pin nor the
routing table was ported — this repo doesn't target a specific MCP
protocol version as policy, and none of those providers exist here (see
`../providers/README.md`). The generic rules were kept.
