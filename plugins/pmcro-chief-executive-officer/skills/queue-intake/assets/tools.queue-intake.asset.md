# Call catalog -- pmcro-chief-executive-officer:queue-intake

All entries here were actually called, live, against this colony's real
`.pmcro/queue/` on 2026-09-05.

## Reading the queue

- `list_directory` (or the host's own directory listing) on
  `.pmcro/queue/` -- returns the loose `task-*.json` files plus the
  `pending/`/`done/` subdirectories. List before reading, since the
  count and names of real items changes as cycles resolve them.
- `read_multiple_files` -- prefer this for reading several
  `task-*.json` files in one round trip over reading them one at a time.
- `read_file` on `.pmcro/queue/seed-intent.schema.json` -- the formal
  shape, kept for reference even though real items diverge from it. Read
  it to know what the ideal shape says, not to expect real items to
  match it.

## The field this actually turns on

- `proposed_role` -- the one field, on the real items observed, that
  functions as a routing hint today, even though it currently only ever
  names a plain execution role. Matching a Chief's own name against this
  field (for example, checking whether any item's `proposed_role` reads
  `chief-executive-officer`) is the whole mechanism -- there is no
  separate "chief" or "team" field to check instead.
- `status` -- `pending`, `claimed`, `in_progress`, `done`, or `rejected`
  per the formal schema; real items observed use `open`-style and `done`
  values consistent with that set. Only an item whose `status` is not
  already `done`/`rejected` is worth surfacing as something waiting.
- `source_trail_id` / `completed_trail_id` -- real trail ids, when
  present, that let a Chief trace an item back to the cycle that filed
  it or the cycle that resolved it, without inventing a link.

## Explicitly out of reach / boundaries

- This skill never mutates a queue item -- no status change, no new
  `proposed_role`, no new file. A Chief that wants to actually claim an
  item does so through its own Orchestrator step, as a TYPE1-adjacent
  decision recorded in that cycle's own `orchestrate.jsonl`, not as a
  side effect of running this skill.
- An empty result (no item's `proposed_role` names this Chief) is not
  evidence the queue mechanism doesn't work -- as of this writing it is
  the honest, expected result, since no real item yet uses a Chief's
  name in that field.
