# Agent Memory

Agent memory is role-scoped retained context and is distinct from shared
Colony memory (`../memory/`) and workflow execution state (`../state/`).

Expected layout:

```text
agent-memory/
├── orchestrator/
├── planner/
├── maker/
├── checker/
└── reflector/
```

Memory entries should be evidence-backed, scoped, redactable, and safe to
reuse. Secrets and raw credentials are never retained here.

## Source and adaptation

Ported verbatim from `pmcro-skills_archive` `.pmcro/agent-memory/README.md`
@ `main` (commit `d864f70`) — genuinely portable, and the five roles
listed are exactly this repo's five roles already. None of the five
subdirectories exist yet; nothing has needed cross-cycle, role-scoped
retained context so far — this repo's roles currently only see what a
sealed trail and the queue hand them at the start of a cycle.
