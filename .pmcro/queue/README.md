# PMCR-O Seed Intent Queue

The queue is a durable file-backed intake boundary. Each Seed Intent is one
JSON document validated against `seed-intent.schema.json`.

## State machine

```text
human input
    |
    v
pending/<task-id>.json
    |
    | deterministic cycle claim + trail link
    v
status: claimed -> in_progress
    |
    | Planner -> Maker -> Checker -> Reflector
    v
status: done + move to done/<task-id>.json
```

`messy_seed` is immutable provenance. Any refined wording belongs in
`canonical_seed`; it must never replace the original human input.

Selection order is deterministic: explicit `-TaskId` first, otherwise
priority ascending, then `created_at`, then task id.

The runtime engine uses an exclusive queue lock so two cycle invocations cannot
claim the same pending item concurrently. It never performs model reasoning.

A claimed/in-progress item is intentionally retained in `pending/` until the
sealed-trail completion path moves it to `done/`; this makes an interrupted
run discoverable for recovery rather than silently losing the work item.
