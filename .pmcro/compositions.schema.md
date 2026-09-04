# PMCR-O Capability Composition Schema

`.pmcro/compositions/` holds records of 2+ existing capabilities used
together to cover a need no single installed provider covers alone — see
`.pmcro/runtime/references/capability-gap-and-composition.md`.

## File naming

`composition-<timestamp>-<slug>.md`, e.g.
`composition-20260904-141534-crlf-safe-diff-scope-check.md`.

## Fields

| Field | Required | Notes |
|-------|----------|-------|
| composition_id | yes | Matches the filename stem |
| status | yes | `candidate` \| `promoted` \| `superseded` |
| proven | yes | `true` when 2+ independent evidence trails are cited, `false` otherwise — see "Source and adaptation" below |
| created_at | yes | ISO-8601 |
| Need (body section) | yes | What this composition covers |
| Composed of (body section) | yes | At least 2 parts — a single capability is not a composition |
| How it composes (body section) | yes | How the parts actually combine |
| Evidence (body section) | yes | At least one source trail where it was exercised |

## `proven` derivation

Mirrors `knowledge-promotion.md`'s own bar ("repeated, independently
checked observations can justify stronger policy"): a composition backed
by only one trail is `proven: false` (still a reasonable `candidate`,
just not yet demonstrated to generalize); two or more independent trails
flips it to `true`.

## Promotion

Per `capability-gap-and-composition.md`: a `proven` composition is a
skill candidate, not yet a first-class capability. Promote it by writing
a `skill-candidate` earned-knowledge record (see
`.pmcro/constraints.schema.md`) that cites this composition, then
scaffold the real skill. Once promoted, set `status: promoted` on this
record rather than deleting it — the composition remains the evidence
trail for the resulting skill.

## Source and adaptation

Ported from `pmcro-skills-lagacy` `.pmcro/compositions.schema.md` @
`main`, at the user's request after they pointed to that repo directly.
One adaptation from the source:

- The source repo derives `proven` automatically via
  `New-PmcroCapabilityComposition`
  (`plugins/pmcro-loop/scripts/new-capability-composition.ps1`), counting
  independent evidence trails for the caller. That plugin does not exist
  in this repo — see `.pmcro/constraints.schema.md`'s "Source and
  adaptation" section for the same underlying gap. `proven` here is a
  manual count the record's author makes and is accountable for getting
  right (count the distinct trail ids actually cited in Evidence), not a
  script-derived, tamper-resistant field.
