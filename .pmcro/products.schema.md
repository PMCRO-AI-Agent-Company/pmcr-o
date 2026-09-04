# PMCR-O Trail Product Schema

`.pmcro/products/` holds Trail Product manifests — packaged, reusable
operational experience per
`.pmcro/runtime/references/trail-as-product.md`. This file documents the
manifest's file shape; deciding whether the source trail(s) actually
validate the product for reuse is Reflector/model reasoning done before
writing one.

## File naming

`product-<timestamp>-<slug>.md`, e.g.
`product-20260904-140733-lease-recovery-workflow.md`.

## Fields

| Field | Required | Notes |
|-------|----------|-------|
| product_id | yes | Matches the filename stem |
| version | yes | Free-form (default `0.1.0`); bump on a materially changed manifest |
| scope | yes | What this product actually covers — narrowest valid framing |
| evidence_class | yes | `native` \| `reconstructed` \| `mixed` — see "Source and adaptation" below on how this is determined in this repo |
| created_at | yes | ISO-8601 |
| Provenance (body section) | yes | At least one source trail id |
| Assumptions (body section) | no | Conditions the product assumes hold in a consumer runtime |
| Known limitations (body section) | no | Where reuse may not hold |
| Reusable skill / marketplace references (body section) | no | Related skills/capabilities this product draws on or could seed |

## Identity binding

A Trail Product manifest never carries or implies consumer execution
identity, operator identity, authority, accounts/resources, or approvals —
see `trail-as-product.md` "Identity binding." The manifest supplies
procedure and evidence; the runtime that re-executes it supplies
everything else.

## Source and adaptation

Ported from `pmcro-skills-lagacy` `.pmcro/products.schema.md` @ `main`, at
the user's request after they pointed to that repo directly. One
adaptation from the source:

- The source repo derives `evidence_class` automatically, via
  `New-PmcroTrailProduct` (`plugins/pmcro-loop/scripts/new-trail-product.ps1`),
  from whether the source trail ids carry a `cycle-` (native) or `retro-`
  (reconstructed) filename prefix. That plugin does not exist in this
  repo, and this repo's trail IDs are plain GUIDs with no such prefix —
  see `.pmcro/constraints.schema.md`'s "Source and adaptation" section for
  the same underlying gap. Until this repo adopts a concrete way to mark
  a trail as retrospective, `evidence_class` here is a manual declaration
  the record's author makes and is accountable for, not a derived,
  tamper-resistant field. As of this port, every sealed trail in this
  repo is native/live, so every product manifest written so far would
  correctly declare `native` by hand.
