---
name: create-skill-test
description: Author and run the PMCRO evaluation contract for an Agent Skill, using deterministic trials, baseline deltas, observable pass criteria, and evidence-backed results. USE FOR — adding or validating eval.yaml for a production skill. DO NOT USE FOR — scaffolding SKILL.md or rewriting failed skill instructions.
metadata:
  version: "0.2.0"
  tier: MAINTAINER
  capability_class: EVALUATION
---

# Create-Skill-Test (pmcro-skills maintainer)

Produce `eval.yaml` beside an existing production `SKILL.md`. The evaluation tests the PMCRO behavior layered on top of the skill's contract — see `references/writing-for-baseline-delta.md` (in the sibling `improve-skill-quality` skill) for what actually earns a delta.

## Inputs

| Input | Required | Description |
|---|---|---|
| Skill path | Yes | Any production skill under `plugins/*/skills/<name>/` |
| SKILL.md | Yes | Source of behavior |
| Baseline notes | Optional | Known failure modes without the skill |

## Evaluation model

Use at least 5 trials; prefer 6. Cover:

1. happy path;
2. a hard Law or Constraint;
3. failure/error handling;
4. role/authority isolation;
5. package/resource behavior;
6. frame/message shape where applicable.

Each trial needs `baseline_gap` and observable `pass_criteria`. Do not mark a trial passed from intent alone.

```yaml
skill: <name>
status: specified_not_yet_run
pass_threshold: "5/6 trials must pass"
trials:
  - id: 1
    prompt: >-
      Concrete agent-style prompt.
    baseline_gap: >-
      Observable difference between unskilled and skilled behavior.
    pass_criteria:
      - "Observable criterion 1"
      - "Observable criterion 2"
```

## Governance

A failing trial is evidence for `improve-skill-quality`; never lower the threshold or delete a failing trial to obtain a pass. Evaluation evidence must remain reproducible and traceable to the skill version.

## Validation

- [ ] `SKILL.md` remains valid.
- [ ] Minimum 5 trials; 6 preferred.
- [ ] Every trial has `baseline_gap` and `pass_criteria`.
- [ ] Results are actually executed before claiming pass.
- [ ] Threshold is explicit.
- [ ] Failure evidence is retained.
