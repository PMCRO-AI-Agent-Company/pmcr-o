# Recipe -- folding queue intake into an Orchestrator step

## For Orchestrator (before minting a brand-new ad hoc seed)

Before writing a fresh `MessySeedIntent` with no prior queue item behind
it, read every real item under `.pmcro/queue/` and check each one's
`proposed_role` against this Chief's own name. If one matches and its
`status` is not already `done`/`rejected`, claim that real item as the
seed instead of inventing a new one -- name its `id` and
`source_trail_id` in the orchestrate frame. If none matches, say so
plainly in the frame ("I checked `.pmcro/queue/`; no item proposes me by
name") rather than silently skipping the check -- the check itself is
what this colony's laws ask for, not a particular outcome from it.

## What this does not replace

A clean queue-intake result reflects the queue's real state at the
moment of the read, not a durable guarantee -- another cycle can file a
new item between this check and the next one. This is a query, not a
lock; it does not claim an item on a Chief's behalf, and it does not
substitute for the claiming cycle's own Orchestrator frame actually
naming the item it claims.
