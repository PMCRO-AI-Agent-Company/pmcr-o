# .pmcro — Colony Runtime

This is the governance layer for the colony's own operation: laws, policy,
capability/provider/MCP routing, config, secrets *references*, and the
runtime contract every governed result must satisfy — plus the two
directories that already existed here and hold this repo's real, live
history:

| Path | What it is | Status |
|---|---|---|
| `laws/` | The colony's fixed rule IDs (`L-EVIDENCE`, `L-CHECKER-GATE`, ...) | mirrored |
| `policies/` | Execution, permission, network, security policy | mirrored |
| `capabilities/` | Capability registry (stable contracts, not tool names) | mirrored, unwired |
| `providers/` | Provider registry (who implements a capability) | mirrored, empty |
| `mcp/` | MCP-specific provider routing | mirrored, empty |
| `config/` | Runtime parameters and environment map | mirrored, adapted |
| `secrets/` | Secret *references* only — never values | mirrored |
| `state/` | Where cross-cycle state would live | mirrored (README only) |
| `memory/` | Where agent/session memory would live | mirrored (README only) |
| `agent-memory/` | Per-agent scoped memory | mirrored (README only) |
| `frames/` | Where frame-type documentation would live | mirrored (README only) |
| `evidence/` | Where evidence artifacts would live | mirrored (README only) |
| `artifacts/` | Where build/output artifacts would live | mirrored (README only) |
| `evaluation/` | Where skill/cycle evaluation results would live | mirrored (README only) |
| `runtime/` | Runtime config, the governed-output contract, orchestrator/harness/durability notes | mirrored, tested |
| `workflows/` | Declarative workflow notes | **not mirrored** — see `workflows/README.md` |
| `queue/` | **Real.** Seed-intent inbox for this repo. | pre-existing, untouched |
| `trails/` | **Real.** Sealed cycle evidence for this repo's own build history. | pre-existing, untouched |

`queue/` and `trails/` are this repo's own live evidence — 16 sealed
trails and 2 queue items as of this mirror, produced by this repo's own
`plugins/pmcro-trail` and `plugins/pmcro-reflector` scripts. This mirror
did not touch, restructure, or add sample content to either directory,
and never will as a side effect of a structural mirror like this one.

## Source and adaptation

Everything else in this directory (`laws/`, `policies/`, `capabilities/`,
`providers/`, `mcp/`, `config/`, `secrets/`, `state/`, `memory/`,
`agent-memory/`, `frames/`, `evidence/`, `artifacts/`, `evaluation/`,
`runtime/`) is ported from [pmcro-skills_archive](https://github.com/PMCRO-AI-Agent-Company/pmcro-skills_archive)
`.pmcro/` @ `main` (commit `d864f70`), a much more mature sibling colony
repo built around Microsoft Agent Framework (MAF), .NET Aspire AppHost,
Docker MCP Toolkit, and a second `pmcro-runtime` repo — none of which
exist here. Direction from the maintainer: **mirror the structure, strip
stack-specific claims.**

What that meant concretely:

- **Kept as-is** where the content is genuinely stack-neutral: `laws.yaml`
  (abstract rule IDs), `policies/permissions.yaml` (role may/mayNot rules
  — a direct, useful match for this repo's own five roles),
  `policies/security.yaml` (trust/secrets/approvals posture, no tool
  names), `runtime/config.yaml` (policy toggles), `runtime/output-contract.md`
  (the governed-result envelope), and `runtime/validate_output_contract.py`
  (the deterministic validator — **run for real against 6 cases** before
  being trusted; see `runtime/README.md`).
- **Adapted** where the archive's content mixed a portable concept with a
  concrete, non-existent-here stack: `policies/execution.yaml` and
  `policies/network.yaml` keep their shape but the capability list is
  marked not-yet-wired; `capabilities/registry.yaml`, `providers/registry.yaml`,
  and `mcp/registry.yaml` keep their schema and rules but the archive's
  concrete entries (GitHub MCP, Playwright MCP, Docker MCP Toolkit,
  `maf-native`, `pmcro-runtime`) were replaced with an explicit "nothing
  configured yet" scaffold rather than invented or copied entries this
  repo can't back up; `config/parameters.yaml` and `config/environments.yaml`
  dropped Aspire parameter-binding syntax, Azure Key Vault, and Docker MCP
  profile/catalog specifics, keeping the generic parameter-reference and
  environment-map shape; `secrets/README.md` kept its "reference, never
  value" contract but genericized the reference table's row names.
- **Not ported**: `runtime/workflows/declarative/pmcro-cycle.yaml` — a
  Microsoft Agent Framework declarative workflow DSL (`InvokeAgent`,
  `ConditionGroup`, `GotoAction`) with no portable content beyond what
  `runtime/config.yaml` already states; this repo's actual Plan→Make→Check→Reflect
  sequence is a human or agent following each role's `run.<name>.asset.md`
  in turn, not a YAML program a runtime executes. `MEMORY.md` and
  `STATE.md` were not ported — both are another session's own operational
  log (specific cycle counts, PR numbers, a specific ML/Cloudflare
  pipeline), not portable content; only their `README.md` companions
  (which describe the *concept* of state/memory storage, not a specific
  session's contents) were kept. `HANDOFF.md`, the root `manifest.yaml`,
  and `AGENTS.md` were not ported — session/stack-specific.
- **Not yet leaf-level**: `capabilities/*.yaml` (browser, containers,
  evidence, filesystem, github, host-command-execution, lifecycle,
  mcp-gateway, memory) and individual `providers/*.yaml` / `mcp/*.yaml`
  files were not created — the registries point at where they'd live, but
  writing fabricated capability/provider definitions for tools this repo
  doesn't have wired would be exactly the kind of invented-integration
  claim `providers/registry.yaml`'s own rules warn against ("Missing
  provider capability produces an escalated governed result, never an
  invented integration"). Add the leaf file for a capability or provider
  when it's real, not ahead of time.

This mirror is a structural/governance layer, not a claim that MAF,
Aspire, Docker MCP Toolkit, or any specific provider is wired into this
repo. Where the archive's content asserted one of those, this mirror
either stripped the assertion or said plainly that nothing is configured
yet.
