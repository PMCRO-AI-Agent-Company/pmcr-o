# PMCR-O O-Mode: Dynamic Resonance

## Definition

`O` is **Dynamic Resonance**: the adaptive layer that selects or changes the strategy through which PMCR-O should pursue the Goal and current Seed Intent.

`Orchestrate` is the runtime behavior that keeps the PMCR cycle moving. O-Mode is broader: it can choose direct execution, optimization passes, options/clarification, chain/tree/graph-style deliberation, ReAct-style observation/action patterns, or future strategies supported by the available capability surface.

The strategy is selected from evidence, not from a permanent hard-coded phase list.

## Strategy selection

The Orchestrator evaluates:

- current Goal and Seed Intent;
- available marketplace capabilities;
- prior trails and strategy history;
- repeated failure signatures;
- constraints and approvals;
- Checker evidence;
- expected value of another cycle.

A strategy can remain unchanged when it is producing progress or change when the current path is unproductive.

## Repeated failure

Repeated failure is evidence for strategy change, not a command to retry forever.

```text
strategy A -> fail
strategy A -> fail
strategy A -> fail
           |
           v
       O-Mode change
           |
           v
strategy B -> test
```

The trail records the strategy transition and its evidence.

## Optimize as repeated application

Optimization, enhancement, and refinement do not have to be separate primitive mechanisms. Multiple successive optimization applications can produce progressively stronger Seed Intents. The resulting strategy and outcomes should be measured in trails rather than assumed.

## Reasoning techniques

Chain-of-thought, tree-of-thought, graph-of-thought, ReAct, and similar techniques are treated as **deliberation strategies**, not as access to or storage of private model chain-of-thought. PMCR-O records the resulting decision artifacts, observations, constraints, and next Seed Intent instead of requiring hidden reasoning transcripts.

## O-Mode output

An O-Mode decision should be attributable and replayable at the strategy level:

```yaml
o_mode:
  selected_strategy: tree
  previous_strategy: direct
  reason: repeated_checker_failure
  evidence:
    failure_signature: ERROR-A
    occurrences: 3
  next_seed_intent: /pmcro-skills:orchestrate evaluate candidate B
```

O-Mode changes how PMCR-O searches; it does not override governance, approvals, or the durable Goal.
