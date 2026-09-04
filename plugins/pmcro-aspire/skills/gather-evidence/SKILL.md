---
name: gather-evidence
description: Query the running .NET Aspire AppHost's live resource state, console/structured logs, and traces via the Aspire MCP server, for use as Make/Check evidence. USE FOR -- confirming a resource is actually Running/healthy, or that a claimed runtime behavior really happened, before writing it into a MakeStep or CheckFrame. DO NOT USE FOR -- mutating resources beyond the single resource-scoped command Aspire itself exposes, reading source code/secrets/env values (excluded by the server), or as a replacement for an actual build/test/HTTP round-trip.
metadata:
  version: "0.1.0"
  tier: EVIDENCE
  capability_class: SUPPORT
---

# Gather Evidence

## Command

No PMCR-O CLI command of its own -- this skill is a recipe for calling the
Aspire MCP server's own tools once registered, not a new deterministic
script. See `assets/setup.gather-evidence.asset.md` for one-time
registration and `assets/tools.gather-evidence.asset.md` for the tool
catalog.

## Purpose

Wraps `aspire agent mcp` (the Aspire CLI's built-in MCP server, STDIO
transport, no auth required) so a Maker or Checker step can attach real,
live evidence -- resource state, console/structured logs, distributed
traces -- instead of relying on what the code is merely supposed to do.
Confirmed current as of the research recorded in trail
`ba0c2c65-075f-470b-abed-e5647053dc8b` (fetched
https://aspire.dev/get-started/aspire-mcp-server/, 2026-09-04).

## Setup

Not yet registered as a live MCP connection in this repo's own agent
runtime -- registering `aspire agent mcp` as an MCP server is a one-time
infrastructure step outside what a deterministic skill script can do (it
edits the calling agent's own MCP client config). See
`assets/setup.gather-evidence.asset.md` for the exact steps and how to
verify the connection with the `doctor` tool.

## Inputs / Outputs / Boundaries

See `assets/tools.gather-evidence.asset.md` for the full tool catalog and
`assets/recipe.gather-evidence.asset.md` for how Maker/Checker should fold
results into `Action`/`Result` or `criteria[].evidence`.

## References

- `assets/setup.gather-evidence.asset.md`
- `assets/tools.gather-evidence.asset.md`
- `assets/recipe.gather-evidence.asset.md`
- `references/README.md`
