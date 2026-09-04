# workflows

The archive's `.pmcro/workflows/declarative/pmcro-cycle.yaml` is
**intentionally not mirrored here.**

## Why

That file is a Microsoft Agent Framework (MAF) declarative workflow
program — `kind: Workflow`, `InvokeAgent`, `InvokeCapability`,
`ConditionGroup`, `GotoAction` — meant to be executed by a MAF workflow
runtime. This repo has no MAF runtime and no engine that executes
declarative workflow YAML. Every governance toggle that file's structure
actually encoded (require evidence, require checker pass, require
reflection before completion, max iterations) is already stated directly,
without a workflow-engine dependency, in `../runtime/config.yaml`.

This repo's real Plan→Make→Check→Reflect sequence is not a YAML program a
runtime interprets — it's a human or agent following each role's
`run.<name>.asset.md` in turn (`plugins/pmcro-orchestrator/skills/orchestrate/`
→ `plugins/pmcro-planner/skills/plan/` → `plugins/pmcro-maker/skills/make/`
→ `plugins/pmcro-checker/skills/check/` → `plugins/pmcro-reflector/skills/reflect-and-seed/`),
documented end-to-end in `.claude/skills/run-pmcro-cycle`. Porting a
non-executable declarative-workflow file into a repo with no engine to
run it would have been exactly the kind of stack-specific claim this
mirror was told to strip, with no offsetting portable content — everything
genuinely portable in that file already lives in `runtime/config.yaml`.

If this repo ever adopts a workflow engine (MAF or otherwise), author a
new declarative workflow against that engine's real syntax then — not by
resurrecting this file.
