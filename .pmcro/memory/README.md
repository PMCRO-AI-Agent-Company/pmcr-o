# Shared Colony Memory

This directory stores durable knowledge available across PMCR-O agents.

- `semantic/` — stable facts and architecture knowledge
- `episodic/` — significant completed events
- `procedural/` — proven procedures and patterns
- `organizational/` — decisions, policies, and institutional knowledge
- `indexes/` — retrieval metadata

Agent-specific retained context belongs under `../agent-memory/<agent>/`
and must not be mixed into shared memory.

Workflow execution state belongs under `../state/`, not memory.

## Source and adaptation

Ported from `pmcro-skills_archive` `.pmcro/memory/README.md` @ `main`
(commit `d864f70`), genuinely portable — no stack-specific claims. One
path was corrected: the archive's own README pointed at
`.pmcro/agents/<agent>/memory/`, but the archive's actual registry
(`capabilities/registry.yaml`) and this mirror both use `agent-memory/`
as the top-level directory name, not `agents/<agent>/memory/` — this copy
points at the path that's actually real in both repos.

The archive's own `MEMORY.md` (a populated instance of this concept —
cycle counts, PR numbers, a specific ML/Cloudflare pipeline) was **not**
ported; it's another session's operational log, not portable content. See
`../README.md` "Source and adaptation".
