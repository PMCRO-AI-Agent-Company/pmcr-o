# Retrospective Trail Reconstruction

Pre-colony work is not automatically part of PMCR-O's accountable history.
A prior chat transcript, a non-PMCR-O agent session's own output, or an
earlier PMCR-O session's own interrupted-session summary all describe real
work, but none of it is queryable Trail history until it is deliberately
reconstructed. This reference defines how to do that reconstruction
without quietly inflating its evidentiary weight to match a native,
live-verified trail.

## What qualifies as a source export

- a pasted or attached prior chat transcript (human-to-agent or agent-to-agent);
- output from a non-PMCR-O agent session working on the same repository;
- a session-continuity summary produced when an earlier PMCR-O session was
  interrupted (the same kind of summary this repo's own runtime has
  received across its own context-compaction boundaries).

## The core discipline: evidenced vs. inferred, never fabricated

A reconstructed trail is not independent verification. It is, at best, an
accurate transcription of what the export itself shows. Two failure modes
to avoid:

- **Fabrication**: inventing plausible-sounding detail (a Checker verdict,
  a specific evidence trail, a clean canonical Seed Intent) that the
  export does not actually contain, because it would make the
  reconstructed trail look more complete. Never do this. Where the export
  is silent, the Frame says so.
- **Overclaiming**: writing a reconstructed CheckFrame as `verdict: pass`
  when the export merely *reports* that a check passed. A reported
  outcome and an outcome this session personally, independently verified
  against current live state are not the same evidentiary strength, and
  the trail must not blur them (see `New-PmcroRetrospectiveTrail`'s
  `INSUFFICIENT SOURCE EVIDENCE` skeleton language and the `reported`
  verdict convention).

This repo's own history already contains a worked example: the
`cycle-20260903-123224-...` and `cycle-20260903-125025-...` trails both
record that 3 of 4 queue items an earlier interrupted session claimed to
have enqueued were, on inspection, never actually written -- the 3
missing items were re-added and explicitly annotated "reconstructed, not
independently re-specified" rather than presented as if a human had
freshly specified them that cycle. That annotation discipline is the same
one this document formalizes for full trail reconstruction.

## Procedure

1. **Allocate the skeleton first, before reading the export closely.**
   Run `plugins/pmcro-loop/scripts/new-retrospective-trail.ps1` (or
   `New-PmcroRetrospectiveTrail`) with a short `-Slug`, a one-line
   `-SourceExport` description (what the export is and how it arrived),
   and a `-ReconstructionBasis` (full transcript vs. summary-only vs.
   partial log -- this bounds how much confidence the reconstruction can
   ever carry). This is deterministic file-mechanics only, mirroring how
   `New-PmcroTrail` allocates a live cycle's skeleton before any Frame
   content is written.
2. **Extract the Seed Intent.** Messy (verbatim, if recoverable) and
   canonical (if the export shows what was actually acted on). If the
   export doesn't clearly support a canonical form, say so rather than
   inventing a cleaner one.
3. **Fill each Frame only as far as the export supports it.** Mark every
   substantive claim `evidenced` (directly supported by export content
   you can point to) or `inferred` (a reasonable reconstruction, flagged
   as such). A Frame with nothing recoverable stays
   `INSUFFICIENT SOURCE EVIDENCE` rather than being padded out.
4. **CheckFrame gets `verdict: reported`, not `pass`/`fail`,** unless this
   session independently re-verified the outcome against current live
   state (in which case that independent verification is itself new,
   native evidence and belongs in a normal live trail referencing the
   retrospective one, not folded silently into it).
5. **Route new work through normal ingress.** If reconstruction surfaces
   genuinely actionable follow-up that isn't already tracked, enqueue it
   through the existing single shared queue (`/send-message` intake or
   `queue-enqueue`) -- a retrospective trail documents history, it does
   not gain a side channel into the colony's backlog.
6. **Seal normally.** `trail_sealed: true` once every Frame section has
   been addressed (filled or explicitly marked insufficient) -- sealing
   means "this reconstruction is as complete as the source allows," not
   "this describes verified live work."

## Relationship to knowledge promotion and Trail Products

`knowledge-promotion.md`'s criteria (recurrence, scope, outcome quality,
**evidence strength**, contradiction with existing knowledge) apply
unchanged to a reconstructed trail, with one addition: evidence strength
for a retrospective Frame is capped by its `reconstruction_basis` and by
whether each claim was marked evidenced or inferred. A reconstructed
trail can still support a provisional constraint or an audit record; it
should not, on its own, justify a strong policy or a skill candidate the
way independently-checked, repeated live-trail evidence can
(`trail-as-product.md`). A skill or constraint drawn from a reconstructed
trail must retain that provenance rather than presenting itself as
equivalent to one drawn from native cycles.

## Relationship to trail-format.md's schema classes

A retrospective trail is Class A (single-file markdown, per-cycle) with
two deliberate deviations from a native Class A trail: the `retro-`
filename prefix (never `cycle-`, so it cannot be mistaken for a
queue-driven cycle) and no `run_id`/queue claim, since it documents a Run
that (if it existed at all) is already over -- there is nothing live to
recover.
