# PMCR-O Intent Lifecycle

PMCR-O treats intent as progressively refined while preserving the original human objective.

```text
Human message
    │
    ▼
Messy Seed Intent
    │
    ▼
Orchestrator establishes Goal/capabilities
    │
    ▼
PMCR-O cycle
Plan → Make → Check → Reflect
    │                         │
    │                         ▼
    │                  next Seed Intent
    │               /[plugin]:[skill] ...
    │                         │
    └─────────────────────────┘
              next cycle

Goal persists across cycles.
Intent lineage records every transition.
Convergence establishes when further refinement is not materially useful.
```

## Responsibility boundaries

The Orchestrator manages the Goal, discovers/resolves capabilities, selects O-Mode strategy, and dispatches the cycle.

The Planner turns the current Seed Intent into the minimum sufficient plan and identifies the capabilities needed to execute it.

The Maker executes permitted work under the governing approval boundary.

The Checker independently evaluates the work and evidence.

The Reflector synthesizes the cycle, records lessons/constraints, and owns the next Seed Intent or terminal disposition.

## Intent states

The first human message is immutable **Messy Seed Intent**. Once the first cycle establishes a canonical operational command, the loop operates on **Seed Intent**. The messy input remains provenance, not the active control instruction.

## Self-reference

Each role writes a Frame that identifies its role and points to its relevant inputs and outputs. A later Frame can trace backward through prior Frames to the evidence that caused its decision. This makes the trail an accountability layer.

## Autonomy

Autonomous operation means the Orchestrator can continue through eligible cycles without the human restating the next Seed Intent because the Reflector supplies it. Autonomy does not bypass TYPE1 approvals, constraints, human handoffs, or the configured cycle limits.

## Continuity

The durable session record is `.pmcro/` plus the installed framework/capability metadata. Chat context may enrich a session but is not required to reconstruct the next cycle.
