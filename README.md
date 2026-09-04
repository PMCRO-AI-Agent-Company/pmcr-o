# pmcro-skills

A self-contained PMCR-O colony skills repository. The repository contains
six lifecycle plugins, maintainer tooling, and the `.pmcro/` governance and
evidence layer used to govern work on the repository itself.

## Runtime products

- `plugins/pmcro-trail/` — durable trail/evidence capability (`initialize`
  skill, `New-Trail` script, Class B GUID-folder trail shape, and the
  Trail-as-Product frame schema every JSONL line conforms to).
- `plugins/pmcro-orchestrator/` — sole dispatch authority (`orchestrate`
  skill), including mint-or-link trail handling.
- `plugins/pmcro-planner/` — turns an opened cycle's task into a PlanFrame.
- `plugins/pmcro-maker/` — executes one PlanFrame step at a time and logs
  the resulting MakeStep.
- `plugins/pmcro-checker/` — independently evaluates MakeSteps against
  PlanFrame success criteria.
- `plugins/pmcro-reflector/` — records disposition, optionally queues a next
  seed, and seals the trail; it is the sole role permitted to seal.

Every production skill package uses `SKILL.md`, optional `assets/`,
`references/`, `scripts/`, and `eval.yaml`. The command-bearing lifecycle
skills expose the repository's command/run/reject asset triad.

## Governance

`.pmcro/` is now an active repository governance layer, not a placeholder.
It contains laws, policies, capability/provider/MCP registries, runtime
configuration and output contract, and durable queue/trail evidence.

The fixed laws include evidence, checker-gate, state/memory separation,
capability use, orchestration ownership, current-evidence research,
plugin isolation, and governed-output conformance. Configurable policies
implement permission, execution, network, security, and approval posture.

The runtime output contract requires structured governance fields for
completion. A deterministic validator exists under `.pmcro/runtime/`.
## Conventions

- No absolute, host-specific, or drive-letter paths in system-authored
  content. References are repository-relative.
- Trail-as-Product: durable trail frames are instances of the declared
  trail-frame schema, not hand-typed prose.
- Queue items likewise use a declared schema and remain separate from the
  skill package.
- Never trust a deterministic script without executing both success and
  failure paths before treating it as complete.
- Every phase file has exactly one owning role; nothing else appends to it.
- Log incrementally as work happens, not as an end-of-cycle reconstruction.
- Sealed trails are immutable. Corrections use a new trail and may reference
  the earlier trail through a next seed.
- Governance configuration must describe real integrations; empty provider
  registries are preferable to invented integrations.
- Secrets are references only and never values in `.pmcro/`.
- Production skill evaluations must retain failing evidence and must not
  lower thresholds to obtain a pass.

## Maintainer tooling

- `.agents/` contains `create-skill`, `create-skill-test`, and
  `improve-skill-quality` for maintaining production skills.
- `.claude/skills/` contains `create-plugin` and `run-pmcro-cycle` for
  repository authoring and governed maintenance procedures.
- `.claude-plugin/marketplace.json` is the canonical marketplace manifest.

## Current scope

The repository has governance scaffolding and real lifecycle plugins, but
its capability/provider/MCP registries remain intentionally empty until a
real provider is wired and verified. The governance layer must not imply
that an unconfigured external integration exists.
