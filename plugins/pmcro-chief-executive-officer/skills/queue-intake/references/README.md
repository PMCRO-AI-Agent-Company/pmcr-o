# References -- pmcro-chief-executive-officer:queue-intake

## Asset map

| Asset | Purpose |
|---|---|
| `assets/setup.queue-intake.asset.md` | the real `.pmcro/queue/` shape as actually observed, and how it diverges from the formal schema |
| `assets/tools.queue-intake.asset.md` | the call catalog, and the one field (`proposed_role`) this mechanism actually turns on |
| `assets/recipe.queue-intake.asset.md` | how an Orchestrator step folds a queue-intake read into its own frame |

## Source and history

Built directly from a real, live read of this colony's own
`.pmcro/queue/` on 2026-09-05: a directory listing, a read of
`seed-intent.schema.json`, and reads of the real `task-*.json` items
already on disk. The gap between the formal schema and the real items'
actual shape was already known before this skill existed (documented
elsewhere in this session's own trail evidence); this skill's
contribution is giving a Chief a disciplined, honest way to query the
real shape as it is, not a proposal to reconcile the two shapes -- that
remains a separate, not-yet-decided piece of work. Update these assets,
not this note, the next time a real queue item actually names a Chief in
`proposed_role` and this skill's "expect an empty result" caveat stops
being true.
