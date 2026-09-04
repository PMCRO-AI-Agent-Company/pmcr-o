---
name: gather-evidence
description: Query the running .NET Aspire AppHost's live resource state, console/structured logs, and traces via the Aspire CLI's local backchannel, for use as Make/Check evidence. USE FOR -- confirming a resource is actually Running/healthy, or that a claimed runtime behavior really happened, before writing it into a MakeStep or CheckFrame. Also owns the safe lifecycle commands (aspire start/stop/wait) that avoid the file-lock and orphaned-process problems `dotnet run` + manual process killing cause. DO NOT USE FOR -- mutating resources beyond a resource's own exposed commands, reading source code/secrets/env values, or as a replacement for an actual build/test/HTTP round-trip.
metadata:
  version: "0.2.0"
  tier: EVIDENCE
  capability_class: SUPPORT
---

# Gather Evidence

## Command

No PMCR-O CLI command of its own -- this skill is a recipe for calling the
Aspire CLI's own commands, not a new deterministic script. See
`assets/setup.gather-evidence.asset.md` for what `aspire agent init`
actually installs and `assets/tools.gather-evidence.asset.md` for the
command catalog.

## Purpose

Wraps the Aspire CLI's local backchannel (a socket at
`~/.aspire/backchannels/` that the CLI uses to talk to a running AppHost)
so a Maker or Checker step can attach real, live evidence -- resource
state, console/structured logs, distributed traces -- instead of relying
on what the code is merely supposed to do.

## Setup

Already done for this repo: `aspire agent init` was run from the repo root
in trail `d360b692-5014-4267-9018-9b94758e9170`'s follow-on verification.
It does **not** register an MCP server (`claude mcp list` confirms none
exist) -- it installs skill files into `.agents/skills/` (`aspire`,
`aspire-init`, `aspire-monitoring`, `aspire-orchestration`,
`aspire-deployment`) that teach the agent to call Aspire CLI commands
directly, plus a `PostToolUse` telemetry hook in this machine's Claude Code
settings. See `assets/setup.gather-evidence.asset.md` for what to do if
`aspire` is missing or a fresh machine needs `aspire agent init` re-run.

## Inputs / Outputs / Boundaries

See `assets/tools.gather-evidence.asset.md` for the command catalog and
`assets/recipe.gather-evidence.asset.md` for how Maker/Checker fold
results into `Action`/`Result` or `criteria[].evidence`.

## References

- `assets/setup.gather-evidence.asset.md`
- `assets/tools.gather-evidence.asset.md`
- `assets/recipe.gather-evidence.asset.md`
- `references/README.md`
