---
name: queue-intake
description: Read the real .pmcro/queue/*.json items and check whether any is already proposed for a specific Chief (by matching proposed_role against that Chief's name) before an Orchestrator step claims a brand-new ad hoc seed. USE FOR -- an Orchestrator-time check of real, durable queue evidence for cross-Chief coordination, consistent with this colony's rule that agents never share live memory. DO NOT USE FOR -- claiming or executing a queue item's actual work (that belongs to the cycle it opens, once claimed), or inventing a proposed_role/tagging convention beyond what a queue item actually states.
metadata:
  version: "0.1.0"
  tier: EVIDENCE
  capability_class: SUPPORT
---

# Queue Intake

## Command

No PMCR-O CLI command of its own -- this skill is a recipe for reading
real files under `.pmcro/queue/`, not a new deterministic script. See
`assets/tools.queue-intake.asset.md` for the read pattern.

## Purpose

Gives a Chief-level Orchestrator step one concrete way to check "does
anyone already need something from me" against real, durable queue
evidence, instead of either presuming nothing is waiting or inventing a
live cross-Chief messaging mechanism this colony's laws don't allow.
`L-STATE-MEMORY` and `L-AGENT-MEMORY` mean no two agents share live
memory or call each other directly; the queue is the one real,
already-existing async surface, and this skill is only a disciplined way
to read it.

## Setup

Nothing to install. Read every `*.json` file directly under
`.pmcro/queue/` (not `.pmcro/queue/pending/` or `.pmcro/queue/done/`,
which this colony's real items do not currently use -- verify this
against the live directory rather than assuming, since colony structure
can change) and load `.pmcro/queue/seed-intent.schema.json` for
reference. See `assets/setup.queue-intake.asset.md` for the exact
sequence.

## Inputs / Outputs / Boundaries

See `assets/tools.queue-intake.asset.md` for the real field this colony's
queue items actually carry (`proposed_role`, not the formal schema's
`domain`/`created_by` fields -- a documented, live discrepancy, not an
error in this skill) and `assets/recipe.queue-intake.asset.md` for how an
Orchestrator step folds a match, or the honest absence of one, into its
own frame.

## References

- `assets/setup.queue-intake.asset.md`
- `assets/tools.queue-intake.asset.md`
- `assets/recipe.queue-intake.asset.md`
- `references/README.md`
