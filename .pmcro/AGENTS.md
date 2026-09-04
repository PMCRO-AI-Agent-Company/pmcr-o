# Colony Runtime Agent Contract

`.pmcro/` is the authoritative runtime and evidence boundary for PMCR-O.

## Rules

- Read repository `AGENTS.md`, `CONTEXT.md`, and `laws.md` before governed mutation.
- Preserve queue, trail, state, memory, policy, and evidence separation.
- Never treat a generated trail as evidence of successful work by itself.
- Never seal a trail outside the Reflector role and Checker gate.
- Keep raw seed provenance immutable.
- Runtime scripts perform deterministic mechanics; domain reasoning belongs to lifecycle roles.

## Storage

- `queue/` — durable Seed Intent intake and lifecycle.
- `trails/` — Class-B evidence products.
- `state/` — current colony state.
- `memory/` — promoted durable knowledge.
- `policies/` — executable governance posture.
- `runtime/` — runtime contracts, references, and deterministic engine.
