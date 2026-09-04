# PMCR-O Runtime Handoff

## Authority

The durable repository state is represented by `.pmcro/manifest.yaml`,
`.pmcro/state/`, `.pmcro/queue/`, and the Class-B trail products under
`.pmcro/trails/`.

## Resume order

1. Read root `AGENTS.md`, `CONTEXT.md`, and `laws.md`.
2. Read `.pmcro/AGENTS.md` and the runtime references required by the active role.
3. Inspect `.pmcro/state/STATE.md` and pending/in-progress queue items.
4. Identify the linked Class-B trail before executing any phase.
5. Resume only from the last durable phase evidence; never reconstruct missing evidence silently.

## Recovery rule

An interrupted `claimed` or `in_progress` task remains discoverable in
`queue/pending/`. The deterministic engine must not silently create a second
trail for the same active claim. Recovery must verify the existing trail and
lease before continuation.

## Completion rule

Checker PASS is mandatory before sealing. Reflector owns disposition/sealing.
After successful sealing, the corresponding task may transition to `done/`.
