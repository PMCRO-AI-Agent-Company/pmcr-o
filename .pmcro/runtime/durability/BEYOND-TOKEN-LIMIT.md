# Durability beyond a single session (runtime notes)

A cycle, or a colony's whole history, can outlive any one agent session
or context window. The durability strategy is: externalize the source of
truth so a new session (or a different agent) can pick up exactly where
the last one left off, without replaying the conversation that produced
it.

## Source and adaptation

Ported and adapted from `pmcro-skills_archive`
`.pmcro/runtime/durability/BEYOND-TOKEN-LIMIT.md` @ `main` (commit
`d864f70`). The archive's version references Temporal/Restate/Cloudflare
Workflows as industry-pattern comparisons, a `pmcro-harness-tick.yml`
scheduled workflow, and "Runtime AppHost: issue #9" — a tracking issue in
the separate `pmcro-runtime` repo. The industry-pattern comparison is
generic enough to keep as background reading; the scheduled-workflow file
and the cross-repo issue reference were dropped — neither exists here.

The core claim is already true of this repo, not aspirational: **this
repo's durability mechanism is git plus `.pmcro/trails/` plus
`.pmcro/queue/`.** A sealed trail's phase files are the durable record of
what happened in a cycle; a queue item is the durable record of what's
next. A new session picks up by reading `.pmcro/queue/` and the most
recent trails, not by being told what happened. That's the same
"externalize source of truth" pattern this note describes — this repo
just doesn't need a scheduled-tick workflow to benefit from it, since
cycles here are run one at a time by whoever's driving, not on a timer.
