# Seed Intent Contract

## Human input versus Seed Intent

A human message is **Messy Seed Intent**. It may be a sentence, a command, a fragment, or an imprecise request. Preserve it exactly for provenance.

A canonical **Seed Intent** is produced for the next cycle by PMCR-O. The Reflector is the default owner of the next-seed handoff after the first cycle.

## Durable capture at the message boundary

`/send-message` is the canonical ingress for a Messy Seed Intent into the colony's one shared queue (`.pmcro/queue.jsonl`). Before any classification reasoning happens, the raw message is durably persisted verbatim as a queue item with `status: intake` (`Add-PmcroIntake` / `plugins/pmcro-loop/scripts/intake-message.ps1`) — deterministic bookkeeping, no model call — so the message survives a session interruption that happens immediately after receipt. Orchestrator later classifies it and resolves the item (`Resolve-PmcroIntake` / `resolve-intake.ps1`) into exactly one disposition: `enqueued` (rewritten into a normal canonical Seed Intent, `status: open`), `informational` (`status: done`, no work follows), or `split` (closed `done`, with each derived item separately enqueued and pointing back via `derived_from_intake`). The original message is preserved permanently in `messy_seed_text` regardless of disposition. See `.agents/commands/send-message.md` and `.pmcro/queue.schema.md`.

An `intake` item that is never resolved (session interrupted mid-classification) is not silently dropped: `session-bootstrap.md` step 3.4 and `engine/run-cycle.ps1`'s Step -1 both scan for it on reconnect and refuse to claim other work until it is classified — the same never-blindly-retry discipline `run-recovery-lease.md` applies to interrupted Runs, applied here to interrupted intake.

`/seed-intent`'s typed API/MCP path is a separate, already-durable ingress and is not affected by this mechanism.

## Canonical command form

```text
/[plugin]:[skill] [optional instructions]
```

The command identifies a marketplace capability. The optional instruction supplies the current operational direction.

Example:

```text
/pmcro-skills:orchestrate inspect the current runtime and continue the PMCR-O goal
```

## Structured representation

```yaml
seed_intent:
  command: /plugin:skill
  plugin: plugin
  skill: skill
  instructions: optional free-form instructions
  source: reflector
  parent_cycle_id: cycle-123
  goal_id: goal-007
  lineage_id: intent-007
```

`command` is the executable address. The remaining fields preserve lineage, provenance, and semantic context.

## Ownership

1. Human supplies Messy Seed Intent.
2. Orchestrator manages the Goal and capability surface.
3. Planner determines the minimum sufficient work and capability requirements.
4. Maker executes the PlanFrame within governance.
5. Checker validates the result and evidence.
6. Reflector synthesizes the cycle and emits the next Seed Intent when another cycle is justified.

The Orchestrator must not manufacture a replacement next Seed Intent in place of the Reflector. It may dispatch the Seed Intent that the Reflector produced.

## Continuation rule

When the next Seed Intent exists, the next PMCR-O cycle consumes that Seed Intent rather than the original Messy Seed Intent. The Messy Seed remains immutable historical provenance.
