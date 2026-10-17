# Claude patterns + MAF native + .agents (full cross-platform)

## What we follow

- **Patterns** from Claude-class agent products (routes, exact tool names, messaging, subagents, grants).
- **Not** locked to one vendor host.

## What we implement

| Layer | Technology |
|-------|------------|
| Convention | `.agents` (pmcro-skills) |
| Runtime | MAF + ProjectName.Orchestrator/Planner/Maker/Checker/Reflector |
| API | ProjectName.OrchestrationApi (`/api/pmcro/request`, `/api/cycles`) |
| Hosts | Claude, Cursor, Codex, Grok, MAF — keep each; activate explicitly |

## Intent pipeline

messy seed → orchestrator goal → planner (validated resources) → maker → checker → **true intent** (reflector)
