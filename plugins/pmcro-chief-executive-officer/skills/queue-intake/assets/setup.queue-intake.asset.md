# Setup -- pmcro-chief-executive-officer:queue-intake

Written from a real, live read of this colony's own `.pmcro/queue/`
(2026-09-05) -- not from the formal schema alone.

## What actually happens, in order

1. **List `.pmcro/queue/`.** It contains loose `task-*.json` files
   directly in the folder, plus two subdirectories, `pending/` and
   `done/`. As of this writing `pending/` is empty and no real item
   lives inside either subdirectory -- every real item sits flat in
   `.pmcro/queue/` itself, tracked by its own `status` field instead.
   Re-check this live rather than assuming it stays true; a future
   reflector-and-seed change could start using the subdirectories for
   real.
2. **Read `seed-intent.schema.json`.** Its formal shape requires `id`,
   `priority`, `domain`, `status`, `created_by`, `messy_seed`,
   `created_at`, with `additionalProperties: false`.
3. **Read the real items.** None of the items actually on disk match
   that schema. Every one observed instead carries: `id`, `summary`,
   `proposed_role`, `source_trail_id`, `created_at`, `status`, and
   sometimes `completed_trail_id` once resolved. This is a real, already
   known discrepancy between the formal schema file and this colony's
   actual `Complete-ReflectAndSeed.ps1` output -- not something this
   skill invents or should paper over.
4. **Check `proposed_role` against the Chief's own name.** As of this
   writing, every real item's `proposed_role` is a plain execution role
   (`maker`, for example) -- none names a Chief. A Chief running this
   skill today will honestly find nothing addressed to it, for every
   real item that currently exists. That is the correct result, not a
   sign the skill is broken.

## What this does not do

This skill never writes to `.pmcro/queue/`, never changes a `status`
field, and never invents a `proposed_role` value that isn't already on
an item. Claiming or acting on an item it finds is the claiming cycle's
own Orchestrator/Planner/Maker work, not this skill's.
