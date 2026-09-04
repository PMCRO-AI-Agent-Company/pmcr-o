---
name: create-skill
description: Scaffold a new PMCRO Agent Skill under a shipped plugin with the native SKILL.md package contract, supporting resources, and evaluation hand-off. USE FOR — authoring a new distributable PMCRO skill. DO NOT USE FOR — editing an existing skill or running evaluations.
metadata:
  version: "0.2.0"
  tier: MAINTAINER
  capability_class: AUTHORING
---

# Create-Skill (pmcro-skills maintainer)

Scaffold one new distributable Agent Skill under the correct PMCRO plugin.

## Plugin targets

- `plugins/pmcro-trail/skills/<name>/`
- `plugins/pmcro-orchestrator/skills/<name>/`
- `plugins/pmcro-planner/skills/<name>/`
- `plugins/pmcro-maker/skills/<name>/`
- `plugins/pmcro-checker/skills/<name>/`
- `plugins/pmcro-reflector/skills/<name>/`

Do not create a new plugin merely to hold a capability already provided by an existing plugin or external MCP server.

## Package shape

Every shipped skill starts with:

```text
<skill-name>/
├── SKILL.md
├── assets/       # optional
├── references/   # optional
└── scripts/      # optional
```

`SKILL.md` is the contract. Supporting resources are loaded progressively.
`eval.yaml` is PMCRO marketplace governance metadata and is required for
production plugin skills.

## Authoring rules

1. Use kebab-case for the directory and matching `name` frontmatter.
2. Write a concise, routing-quality `description` describing when the skill should load.
3. Keep detailed material in `references/`; static schemas/templates in `assets/`; deterministic executable logic in `scripts/`.
4. Never create a custom Skill loader, queue directory, or runtime state directory inside the skill.
5. If the skill uses MCP, tools, or shell execution, document the approval and acceptability boundary.
6. Add `eval.yaml` and ensure the skill is covered by repository CI.
7. If the skill exposes a command surface, give it the `command.<name>.asset.md` / `run.<name>.asset.md` / `reject.<name>.asset.md` triad — see any existing `plugins/pmcro-*/skills/*/` for the pattern; do not retype it.

## Validation

- [ ] Native `SKILL.md` exists and has valid frontmatter.
- [ ] Only `SKILL.md`, `assets/`, `references/`, `scripts/`, and `eval.yaml` are present in production skill directories.
- [ ] Resources are referenced by name/path and are suitable for progressive disclosure.
- [ ] Executable scripts are trusted, tested for real, and approval-gated.
- [ ] PMCRO role/frame ownership is explicit where applicable.
- [ ] No runtime queue, trail instance, memory, or mutable state is embedded in the package.
- [ ] `eval.yaml` exists for production skills.
- [ ] Repository verification passes.
