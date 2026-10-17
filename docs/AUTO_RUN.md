# Auto-run mode (Claude-like continuous loop under PMCR-O)

Claude Code “auto” / accept-edits style execution is allowed **inside an admitted phase** only.

## Allowed

- Continuous tool/MCP steps within MAKE (or read-only steps within CHECK) up to `MaxStepsPerPhase`
- Parallel host subagents with `parent_tool_use_id` / `agent_id`
- Progress without human keypress **after** ORCHESTRATE admission and **after** framework activation

## Forbidden

- Skipping CHECK
- Maker self-approving (Checker identity must differ)
- Silent framework activation
- Auto-executing Reflector `NextSeedIntent`
- Treating projection or MCP catalog discovery as grant

## Message convention

Still: `@maker /pmcro:make` (human) + `PmcroRequest` (machine).  
Auto-run is an **option on the runtime**, not a new phase.
