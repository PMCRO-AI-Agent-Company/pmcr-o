# From Trail to Knowledge

A trail is experience. Knowledge promotion turns selected experience into reusable system knowledge without treating every event as a permanent rule.

```text
Trail Frames
    |
    v
Pattern / evidence review
    |
    +--> constraint
    +--> rule / policy
    +--> O-Mode strategy evidence
    +--> skill candidate
    +--> training example
    +--> evaluation case
    +--> audit-only record
```

## Promotion criteria

Promotion should consider recurrence, scope, outcome quality, evidence strength, and contradiction with existing knowledge. A trail reconstructed from a historical/third-party export (see `retrospective-trail-reconstruction.md`) has evidence strength capped by how much of it was directly evidenced versus inferred — it can support a provisional constraint or audit record, but should not alone justify a strong policy or skill candidate the way independently-checked, repeated live-trail evidence can.

A single failure can be a provisional constraint or evaluation case. Repeated, independently checked observations can justify stronger policy. Reusable successful procedures can become skill candidates.

## Skill synthesis

A trail itself is not a skill. It is an instance of experience from which a skill can be generalized.

Once a promotion judgment is actually made, `New-PmcroConstraint` (`plugins/pmcro-loop/scripts/new-constraint.ps1`) writes the record deterministically under `.pmcro/constraints/` — see `.pmcro/constraints.schema.md`. The function enforces only that the record cites at least one trail as evidence; it does not make the promotion judgment itself.

A capability composition (2+ existing capabilities used together to cover a need no single installed provider covers alone — see `capability-gap-and-composition.md`) is a specific, structured input to this same skill-candidate path: a composition proven across repeated, independently checked trails (`New-PmcroCapabilityComposition`'s auto-derived `proven` field) is promoted the same way any other skill candidate is — a `skill-candidate` constraint record citing it, then `/createskill`. A recorded, still-`open` capability gap (`New-PmcroCapabilityGap`) is the negative case: a need with no covering capability or composition yet, kept as a durable lead rather than re-discovered from scratch each time.

```text
one trail -> experience
multiple related trails -> pattern
pattern + validated procedure -> skill candidate
skill candidate -> skill creation / validation
validated skill -> marketplace capability
```

Skill generation must preserve provenance to the supporting trails and must be checked before becoming an active capability.

## Model improvement

Trail Frames can form structured training or evaluation data for future PMCR-O model improvement. Data selection must distinguish useful learning examples from private, transient, sensitive, or merely historical information. Never assume that every trail belongs in a fine-tuning dataset.

A future learning pipeline may use .NET Aspire and Microsoft Agent Framework for runtime orchestration, with Python or other services for offline analysis, evaluation, or model-training workflows.

The PMCR-O runtime should remain useful without model fine-tuning. Learned models are an optimization of O-Mode and capability selection, not a prerequisite for the core loop.
