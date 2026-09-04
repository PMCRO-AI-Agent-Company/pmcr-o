---
name: create-custom-agent
description: Scaffold a bounded PMCR-O agent definition under a plugin agents directory or maintainer agents directory. Use when creating a new role; do not use for lifecycle execution or domain mutation.
metadata:
  version: "1.0.0"
  tier: MAINTAINER
  capability_class: AUTHORING
---

# Create Custom Agent

## Outcome
Create one role definition that is explicit about identity, authority,
skills, inputs, outputs, and safety boundaries.

## Procedure
1. Choose `plugins/<plugin>/agents/<name>.agent.md` for shipped roles or
   `.agents/agents/<name>.agent.md` for maintainer-only roles.
2. Read `AGENTS.md`, `CONTEXT.md`, and `laws.md` before authoring.
3. Declare a small frontmatter block with `name`, `description`, `tier`, and
   `tools` when tools are known.
4. Define responsibilities, allowed phase, required inputs, output contract,
   validation, and explicit non-goals.
5. Reference existing skills instead of copying their instructions.
6. Ensure the agent cannot route around the Orchestrator or Checker gate.

## Required boundaries

- Never invent a provider, MCP server, credential, or runtime integration.
- Never perform an out-of-phase mutation.
- Never seal a trail unless the role is the authorized Reflector path.
- Never write partial files; use the Atomic File Protocol.
- Use repository-relative paths only.

## Validation
Run `tests/run-evals.ps1` and the repository's JSON/YAML/PowerShell
validation before treating the new agent as production-ready.
