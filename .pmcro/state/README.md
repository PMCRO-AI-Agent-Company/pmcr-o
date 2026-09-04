# Runtime State

State is operational and ephemeral/durable execution context, not
knowledge memory.

- `schemas/` — state contracts
- `runs/` — execution instances
- `checkpoints/` — resumable workflow checkpoints
- `locks/` — concurrency/ownership metadata

Do not place long-term agent knowledge here.

## Source and adaptation

Ported verbatim from `pmcro-skills_archive` `.pmcro/state/README.md` @
`main` (commit `d864f70`) — genuinely portable, no stack-specific claims.
None of the four subdirectories it describes exist yet in this repo; this
is documentation of the concept, not a claim any of them are populated.
This repo's actual "resumable" mechanism today is `.pmcro/trails/`
itself — a sealed trail's phase files already are the durable record of
where a cycle got to — so a `state/` layer would only earn its keep once
something needs mid-cycle (not just sealed-cycle) resumability.
