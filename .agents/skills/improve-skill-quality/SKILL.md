---
name: improve-skill-quality
description: Diagnose failed evaluation trials for an Agent Skill and patch the smallest behaviorally correct surface until structural validation and the PMCRO evaluation threshold pass. USE FOR — failed or weak production skills under any plugins/*/skills path. DO NOT USE FOR — scaffolding a new skill or authoring the initial eval.yaml.
metadata:
  version: "0.2.0"
  tier: MAINTAINER
  capability_class: AUTHORING
---

# Improve-Skill-Quality (pmcro-skills maintainer)

Fix a production Skill that fails structural validation or falls below its evaluation threshold. Diagnose from concrete failed criteria; do not merely add prose. See `references/writing-for-baseline-delta.md` before touching a `SKILL.md` — most failures are padding, not missing information.

## Failure classes

- **Structural** — invalid frontmatter, missing `SKILL.md`, or unsupported package entries.
- **Resource** — references/assets/scripts are missing, incorrectly named, or not progressively discoverable.
- **Behavioral** — the skilled arm violates an observable evaluation criterion.
- **Governance** — Law, Constraint, role isolation, or evidence boundaries are violated.

## Workflow

1. Read `SKILL.md` and its referenced resources.
2. Identify the smallest failing behavior.
3. Patch the minimum surface: `SKILL.md`, resource, script, or schema.
4. Never create a custom loader or runtime queue inside the Skill.
5. Re-run structural validation and affected evaluation trials.
6. Keep failed evidence and do not lower thresholds to obtain green status.

## Validation

- [ ] Skill package remains valid.
- [ ] Failed criteria are explicitly addressed.
- [ ] No runtime state was embedded in the Skill.
- [ ] Evaluation threshold is met without deleting hard trials.
- [ ] Description remains routing-quality.

## References

- `references/writing-for-baseline-delta.md`
