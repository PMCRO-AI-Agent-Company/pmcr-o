# Capability Gap and Composition

`discover-capabilities`'s Resolution contract step 5 says: "If no
provider is installed, report unresolved capability rather than inventing
one." That's the right instinct but an incomplete procedure — it neither
tries composing existing capabilities before giving up, nor durably
records the gap so the next cycle doesn't re-search from scratch. This
reference fills in both steps, and connects the result to
`knowledge-promotion.md`'s existing skill-candidate pipeline.

## The procedure

1. **Discover.** Run `discover-capabilities`
   (`.agents/skills/discover-capabilities/scripts/discover-capabilities.ps1`
   + `resolve-capability.ps1`) against the resolved projects root. This
   already does partial, scored matching — a zero-score result and a
   low-score partial match are both "no confident single provider," not
   automatically a gap.
2. **Prefer an exact installed provider.** If discovery resolves one with
   reasonable confidence, use it. Nothing below applies.
3. **If only partial matches exist, look for a composition.** Before
   concluding the need is unmet, check whether 2 or more of the partial
   matches (or other already-available tools/capabilities) used together
   actually cover it. This is Planner/model reasoning, not something
   `discover-capabilities` itself computes.
4. **If a composition works, record it** with `New-PmcroCapabilityComposition`
   (`plugins/pmcro-loop/scripts/new-capability-composition.ps1`) — see
   `.pmcro/compositions.schema.md`. Cite the trail where it was actually
   exercised. `proven` is derived automatically from the evidence count
   (2+ independent trails), never asserted by the caller.
5. **Only if no composition suffices either, record the gap explicitly**
   with `New-PmcroCapabilityGap`
   (`plugins/pmcro-loop/scripts/new-capability-gap.ps1`) — see
   `.pmcro/capability-gaps.schema.md`. The function refuses to write a gap
   record without a non-empty explanation of why composition was
   considered and rejected — a gap is not a shortcut around step 3.
6. **On reconnect, an open gap is a lead, not noise.** A future cycle
   facing a similar need should check `.pmcro/capability-gaps/` for an
   open, matching record before re-running the same search from nothing —
   this mirrors the discipline behind unresolved intake
   (`seed-intent-contract.md`) and recoverable Runs
   (`run-recovery-lease.md`): a durably-captured signal is never silently
   re-derived or dropped.

## Promotion into a first-class capability

A composition that keeps proving itself (repeated, independently checked
evidence — `knowledge-promotion.md`'s own bar) is a skill candidate, not
yet a capability. The path from there reuses cycle 4's mechanism rather
than inventing a new one:

1. Write a `skill-candidate` earned-knowledge record
   (`New-PmcroConstraint -Kind skill-candidate`) that cites the
   composition record and the trails proving it.
2. Scaffold the real skill with `/createskill`
   (`INSTRUCTIONS.md`: "use it instead of hand-rolling SKILL.md
   frontmatter").
3. Once the skill exists and is installed, `discover-capabilities` will
   find it directly on the next scan — the composition and any open gap
   record it resolved should be marked `promoted` /
   `resolved` (`resolved_by: <new skill name>`) rather than left to look
   perpetually open.

## What this deliberately does not do

- It does not make `discover-capabilities` itself attempt or evaluate
  compositions — that stays Planner/model reasoning, matching the
  boundary every other classification decision in this engine already
  respects.
- It does not auto-detect when a composition has "proven itself enough"
  to promote — a human or Reflector judgment call, same as every other
  promotion decision in `knowledge-promotion.md`.
- It does not fabricate a gap or a composition without real evidence — an
  unevidenced gap is exactly the kind of untested workaround
  `knowledge-promotion.md` warns against treating as a universal rule.
