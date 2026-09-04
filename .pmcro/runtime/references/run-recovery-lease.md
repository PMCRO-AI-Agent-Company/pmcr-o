# Run, Checkpoint, Recovery, and Lease

Live execution continuity is a distinct concern from Trail history. A Trail
answers "what happened and why." A **Run** answers "is work still in
flight, and is it safe to touch."

This contract strengthens existing structures rather than adding a parallel
registry: a claimed queue item already *is* a Run identity.

## Run

A Run is identified by the queue item it was claimed against
(`task_id` doubles as `run_id`). No new store is introduced. A claimed
`.pmcro/queue.jsonl` item gains these optional fields:

```json
{
  "lease_owner": "orchestrator@<runtime-instance-id>",
  "lease_expires_at": "2026-09-03T05:45:00Z",
  "heartbeat_at": "2026-09-03T05:15:00Z",
  "checkpoint_ref": ".pmcro/checkpoints/task-<slug>.md"
}
```

Absence of these fields means the item predates this contract or has no
live runtime attached; it is treated as a normal claimed item, not a Run.

## Checkpoint

A Checkpoint is a small durable file at `checkpoint_ref`, one per active
Run, holding just enough to reconstruct in-flight state without the chat
transcript:

```yaml
task_id: task-<slug>
phase: maker            # orchestrator|planner|maker|checker|reflector
last_completed_step: "generated AppShell"
in_progress_operation: "dotnet build"
external_state_expected: "unknown - not yet observed post-interruption"
last_frame_id: frame-113
updated_at: 2026-09-03T05:20:00Z
```

Checkpoints are working/durable state, not accountability records. They
may be deleted once a Run reaches a terminal Trail disposition (accept,
reject, or superseded); the Trail is what persists afterward.

## Recovery

**Invariant: an interrupted operation is never retried blindly.** Before
resuming a Run whose heartbeat has gone stale, the resuming runtime must:

1. Read the checkpoint to learn `in_progress_operation` and
   `last_completed_step`.
2. Inspect actual state relevant to that operation (git status/diff, file
   existence, build/test output, process/service state) rather than
   assuming success or failure.
3. Classify the operation as one of:
   - **resume** — untouched or clearly incomplete; continue from
     `last_completed_step`.
   - **compensate** — partially applied in a way that must be undone or
     completed idempotently before continuing.
   - **retry** — safe to redo from scratch (side-effect-free or already
     confirmed not to have applied).
4. Record the interruption, the inspection evidence, the recovery
   decision, and the outcome as Frame content in the Trail for that
   cycle (see `trail-frame-schema.md`).

A stale heartbeat is evidence of possible interruption, not evidence of
what actually happened externally — step 2 is mandatory and may not be
skipped because a checkpoint says a step was "in progress."

## Lease / heartbeat

Only one runtime may act on a given Run at a time.

- On claim, the claiming runtime writes `lease_owner`, `heartbeat_at` (now),
  and `lease_expires_at` (short TTL, e.g. now + 30m).
- The active runtime refreshes `heartbeat_at`/`lease_expires_at` while
  working the Run.
- A different runtime may treat a Run as recoverable only when
  `lease_expires_at` has passed. It must not simply reclaim on a stale
  heartbeat without going through Recovery above, and must overwrite
  `lease_owner` to itself only after that inspection.
- This is deterministic bookkeeping (timestamps/ownership), not a model
  judgment call, matching how `queue-claim` already assigns `claimed_by`.

## Approval boundary across interruption

Recovery must never infer or extend a TYPE1 approval from a stale or
interrupted Run. `approvals.schema.md`'s enforcement rules apply
unchanged: an approval record must still exist, be unexpired, and cover
the exact operation/scope being resumed. An interrupted TYPE1 mutation
that lacks a still-valid approval record fails closed and returns to
`needs-human-approval`, regardless of how far it progressed.

## Relationship to Trail/Frame

- **Run** (queue item + checkpoint): live, mutable, only meaningful while
  work is in flight. Deleted/ignored once the Trail is sealed.
- **Trail/Frame**: historical, append-only, permanent. Frames may carry
  `run_id`, `checkpoint_ref`, and `recovery_decision` (see
  `trail-frame-schema.md`) so a sealed Trail explains any interruption
  that occurred during its cycle, but the Trail does not require a live
  Run to exist or be read.

## Implementation

This contract is wired into the deterministic (non-LLM) `pmcro-loop` engine,
not left purely documentary:

- `plugins/pmcro-loop/engine/PmcroEngine.psm1` — `Claim-PmcroTask` writes the
  initial lease/checkpoint on claim; `Update-PmcroLease` refreshes
  heartbeat/lease and refuses to transfer an unexpired lease to a different
  owner (prevents simultaneous ownership); `Test-PmcroLeaseStale` /
  `Find-PmcroRecoverableRuns` detect a stale lease deterministically without
  classifying it; `Set-PmcroCheckpoint` / `Get-PmcroCheckpoint` /
  `Remove-PmcroCheckpoint` manage the Checkpoint file; `Complete-PmcroRun`
  closes a Run at terminal disposition.
- `plugins/pmcro-loop/engine/run-cycle.ps1` — scans for a recoverable Run
  before claiming anything new, and STOPs with checkpoint + git-status
  evidence rather than resuming/retrying/compensating itself.
- `plugins/pmcro-loop/scripts/heartbeat.ps1`, `checkpoint.ps1`,
  `complete-run.ps1` — callable by any role (engine-run or hand-executed
  per `queue-claim`/`make-frame`/`reflect-and-seed` SKILL.md) to refresh a
  lease, record a checkpoint, or close a Run without needing to hand-edit
  `queue.jsonl`.

None of the above calls a model or makes a resume/compensate/retry
classification -- that judgment stays with the Planner/Orchestrator per
Recovery above.

## Knowledge promotion

A validated recovery decision (e.g. "a `dotnet build` interrupted after
process start must be re-verified via `dotnet build --no-incremental`
before treating it as failed") is ordinary Reflector output and follows
the existing constraint/rule/strategy/skill-candidate path in
`accountability-and-trails.md` and `knowledge-promotion.md`. No separate
promotion mechanism is introduced for recovery knowledge.
