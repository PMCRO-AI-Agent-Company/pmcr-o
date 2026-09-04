# Trail as Product

A Trail Product packages validated operational experience for reuse in another runtime.

The product is the methodology and evidence encoded by the trail, not the creator's identity, credentials, accounts, or execution authority.

```text
Creator runtime
    |
    v
Validated trails
    |
    v
Trail Product
    |
    +--> consumer runtime
    |       +--> consumer identity
    |       +--> consumer authority
    |       +--> consumer accounts/resources
    |       +--> consumer approvals
    |
    v
re-execution + new evidence
    |
    v
new trails
```

## Reuse

A Trail Product may carry:

- proven workflows and sequences;
- strategy transitions and their evidence;
- constraints and their scope;
- acceptance checks;
- failure/recovery knowledge;
- references to reusable skills and marketplace capabilities.

It must not imply that a consumer inherits the creator's authority or identity.

## Identity binding

Use terms such as **execution identity**, **operator identity**, or **identity binding** rather than identity injection for the runtime association between a reusable trail and the actor that executes it.

The runtime supplies the credentials and approvals needed for execution. A trail supplies learned procedure and evidence.

## Product lifecycle

```text
experience -> trail -> validation -> product -> reuse -> new evidence -> improved trail
```

Trail Products should retain provenance to their source trails and declare version, scope, assumptions, and known limitations.

A source trail reconstructed from a historical/third-party export (see `retrospective-trail-reconstruction.md`) carries weaker evidence than a native live trail and that weakness must be declared as part of the product's provenance, not smoothed over.

A Trail Product manifest lives under `.pmcro/products/` — see `.pmcro/products.schema.md`, including that schema's "Source and adaptation" note: this repo has no `plugins/pmcro-loop` script to derive `evidence_class` automatically, so it is a manual declaration the record's author is accountable for, not a tamper-resistant derived field.
