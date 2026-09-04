# laws

Fixed rule IDs the colony's roles and mechanics are expected not to
violate. A law is an ID and a machine-readable rule name, not prose policy
(that's `../policies/`) and not enforcement code (that's `../runtime/`).

Ported verbatim from `pmcro-skills_archive` `.pmcro/laws/laws.yaml` — see
`../README.md`'s "Source and adaptation" section. No content here is
stack-specific, so nothing was stripped.

## How these map onto this repo today

Most of these already describe what this repo's real scripts and
INSTRUCTIONS.md enforce, even though this repo doesn't (yet) call them by
these IDs:

- `L-EVIDENCE` / `L-CHECKER-GATE` — this repo's own Check phase
  (`plugins/pmcro-checker`) already gates Reflector's disposition on a
  pass/fail verdict; a cycle can't seal without one.
- `L-ORCHESTRATION` — matches `plugins/pmcro-orchestrator`'s stated role:
  sole dispatch authority, not a domain implementer.
- `L-PLUGIN-ISOLATION`'s `TYPE1` term is the same `TYPE1` the project's
  own `INSTRUCTIONS.md` approval protocol already uses for state-changing
  mutations requiring explicit approval — this law is naming a rule this
  repo already follows in practice.
- `L-STATE-MEMORY`, `L-AGENT-MEMORY`, `L-CAPABILITY`, `L-RESEARCH` are not
  yet enforced by any script here — they're aspirational until this repo
  actually has shared workflow state, per-agent memory, or a capability
  layer to violate. Keeping the law ID now, ahead of the enforcement,
  means a future implementation has a name to build toward instead of
  inventing one under time pressure.
