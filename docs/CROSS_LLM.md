# Cross-LLM conventions

| Host | Projection folder | Activation |
|------|-------------------|------------|
| Claude Code | `.claude` | explicit |
| Codex | `.agents/skills` native | explicit |
| Cursor | `.cursor` + `.agents` | explicit |
| Grok | `.grok` (when verified) | explicit |
| Gemini / other | declared unverified | explicit |

Canonical surface: `.agents`. Tool vocabulary prefers Claude names; adapters map.  
Template remains `ProjectName.*`; marketplace remains `pmcro-skills`.
