# PMCR-O Capability Gap Schema

`.pmcro/capability-gaps/` holds durable records of a need that neither an
installed capability nor a composition of existing ones covers — see
`.pmcro/runtime/references/capability-gap-and-composition.md`.

## File naming

`gap-<timestamp>-<slug>.md`, e.g.
`gap-20260904-141719-offline-pdf-ocr.md`.

## Fields

| Field | Required | Notes |
|-------|----------|-------|
| gap_id | yes | Matches the filename stem |
| status | yes | `open` \| `resolved` |
| resolved_by | no | Set when `status: resolved` — the capability/skill name that now covers `Need` |
| discovery_query | no | What was searched for/tried, if anything, before concluding no provider fits |
| created_at | yes | ISO-8601 |
| Need (body section) | yes | What was being sought |
| Partial matches found (body section) | no | Weak/partial provider matches found, if any — `(none found)` if truly zero |
| Why composition doesn't suffice (body section) | yes | A gap is not a shortcut around trying composition first — see the procedure below |
| Evidence (body section) | yes | At least one trail documenting the search |

## Hard rule

A gap record requires a non-empty "Why composition doesn't suffice"
explanation — see "Source and adaptation" below on how this is enforced
in this repo.

## Resolution

A gap stays `open` until a capability that actually covers `Need`
exists — an installed provider found later, a composition later proven,
or a newly scaffolded skill. Set `status: resolved` and `resolved_by` at
that point rather than deleting the record; the gap's own history (that
this need went unmet for a time) is evidence worth keeping.

## Source and adaptation

Ported from `pmcro-skills-lagacy` `.pmcro/capability-gaps.schema.md` @
`main`, at the user's request after they pointed to that repo directly.
Two adaptations from the source:

- The source repo enforces the hard rule via `New-PmcroCapabilityGap`
  (`plugins/pmcro-loop/scripts/new-capability-gap.ps1`), which refuses to
  write a record without the composition explanation. That plugin does
  not exist in this repo — see `.pmcro/constraints.schema.md`'s "Source
  and adaptation" section for the same underlying gap. A gap record here
  is authored directly as markdown, and the author is responsible for the
  hard rule holding, not the tooling.
- The source repo's `discovery_query` field names a specific script,
  `resolve-capability.ps1` (part of a `discover-capabilities` skill this
  repo does not have — this repo's marketplace discovery is the four
  `.claude-plugin`/`.cursor-plugin`/`.agents/plugins`/`.codex-plugin`
  manifests, browsed directly, not a scripted resolver). The field is
  kept, but reworded to describe whatever was actually searched or tried,
  since there is no `resolve-capability.ps1` output to cite here.
