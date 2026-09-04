# PMCR-O Runtime References

These references are the recovered semantic foundation for the PMCR-O runtime.
They were ported byte-for-byte from the authoritative legacy framework at
`P:/source/pmcro-skills`.

## Foundation set

- `o-mode.md` — Dynamic Resonance and strategy selection.
- `run-recovery-lease.md` — Run identity, checkpoint, recovery, and lease.
- `retrospective-trail-reconstruction.md` — accountable reconstruction of prior work.
- `seed-intent-contract.md` — messy seed provenance and canonical seed handoff.
- `intent-lifecycle.md` — progressive intent refinement.
- `trail-as-product.md` — durable Trail Product semantics.
- `capability-gap-and-composition.md` — capability composition and durable gaps.
- `knowledge-promotion.md` — promotion of validated trail experience to knowledge.
- `session-bootstrap.md` — runtime session initialization order.
- `context-budget.md` — package/context budgeting and truncation discipline.

## Provenance

The nine foundation references under `plugins/pmcro/skills/foundation/references/`
and `context-budget.md` under `plugins/pmcro/skills/package/references/` were
copied without semantic rewriting. SHA-256 parity was verified against the
legacy source before this index was added.

The runtime engine is deliberately separate from these semantic references:
it performs deterministic file mechanics only and does not replace the
existing Orchestrator, Planner, Maker, Checker, or Reflector contracts.
