# PMCR-O Earned Knowledge Schema

`.pmcro/constraints/` holds durable, evidence-backed records promoted from
Trail experience — see `.pmcro/runtime/references/knowledge-promotion.md`
for the promotion criteria (recurrence, scope, outcome quality, evidence
strength, contradiction with existing knowledge) that must be satisfied
*before* a record is written here. This file documents the record's file
shape once that judgment has been made.

Distinct from a foundational, hand-authored colony law document (this
repo has no `runtime-baseline.md` equivalent yet) — a constraint record is
a per-cycle earned record, not baseline policy.

## File naming

`<kind>-<timestamp>-<slug>.md`, e.g.
`constraint-20260904-140733-array-return-wrapping.md`.

## Fields

| Field | Required | Notes |
|-------|----------|-------|
| constraint_id | yes | Matches the filename stem |
| kind | yes | One of: `constraint`, `rule-policy`, `strategy-preference`, `skill-candidate`, `training-example`, `audit-record` — mirrors the taxonomy in `knowledge-promotion.md` |
| scope | yes | The narrowest valid scope the evidence actually supports |
| status | yes | `provisional` \| `active` \| `superseded` |
| superseded_by | no | Set when `status: superseded`; the id of the record that replaced this one |
| created_at | yes | ISO-8601 |
| Statement (body section) | yes | The actual rule/observation/candidate being recorded |
| Evidence (body section) | yes | At least one source trail id |

## Hard rule

An earned record must cite at least one trail as evidence. An unevidenced
"earned" record contradicts the word "earned."

## Scope discipline

Widening a record's `scope` beyond what its cited evidence demonstrated is
not an in-place edit. Write a new record with the wider scope (backed by
whatever new evidence justifies the widening) and set the old record's
`status` to `superseded` with `superseded_by` pointing at the new one.
History is preserved, never overwritten.

## Reconstructed evidence

A record whose evidence trail(s) are retrospective reconstructions (see
`.pmcro/runtime/references/retrospective-trail-reconstruction.md`) carries
capped evidence strength per `knowledge-promotion.md` — it can support
`provisional`/`audit-record` but should not alone justify `active`
`rule-policy`/`skill-candidate` status the way independently-checked,
repeated native-trail evidence can.

## Source and adaptation

Ported from `pmcro-skills-lagacy` `.pmcro/constraints.schema.md` @ `main`,
at the user's request after they pointed to that repo directly. Two
adaptations from the source:

- The source repo names this "hard rule" and its `evidence_class`-style
  derivations as enforced by a deterministic script,
  `New-PmcroConstraint` (`plugins/pmcro-loop/scripts/new-constraint.ps1`).
  That plugin does not exist in this repo (confirmed: `plugins/` here has
  no `pmcro-loop` directory), and none of this repo's existing phase
  scripts (`New-Trail.ps1`, `New-OrchestrateFrame.ps1`, etc.) cover it
  either. In this repo, a constraint record is authored directly — by the
  Reflector role, as part of `reflect-and-seed`, or by whichever role
  made the promotion judgment — as a plain markdown file matching this
  schema. The hard rule above (cite at least one trail) is therefore a
  discipline the author is responsible for, not something the tooling
  enforces. If this pattern gets used often enough that hand-authoring it
  correctly becomes error-prone, writing an actual script is itself a
  skill-candidate constraint record, decided the same way any other
  promotion is.
- The source repo's evidence-strength distinction between native and
  reconstructed trails is keyed off a `cycle-`/`retro-` filename prefix
  convention its trail IDs use. This repo's trail IDs are plain GUIDs
  with no such prefix (e.g. `b6299719-6309-4df2-a208-83e891f8d4ad`) — as
  of this port, every sealed trail in this repo is native/live, so the
  distinction has not yet needed a concrete marker here. If a
  retrospective trail is ever reconstructed in this repo, it will need
  its own way of being marked as such (a disposition field, a directory,
  or an adopted filename convention) before this section's guidance can
  be applied precisely — not yet defined.
