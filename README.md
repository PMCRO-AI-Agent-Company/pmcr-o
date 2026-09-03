# pmcro-skills (fresh)

A clean, from-scratch PMCR-O colony repo. Git-initialized with no history or
content carried over from any other checkout on this host — this repo is
entirely self-contained.

## What's here so far

- `plugins/pmcro-trail/` — durable trail/evidence capability (`initialize`
  skill, `New-Trail` script, Class B guid-folder trail shape, and the
  Trail-as-Product frame schema every jsonl line conforms to).
- `plugins/pmcro-orchestrator/` — sole dispatch authority (`orchestrate`
  skill), convenient mint-or-link trail-id contract.
- `plugins/pmcro-planner/` — turns an opened cycle's task into a PlanFrame
  (`plan` skill).
- `plugins/pmcro-maker/` — executes one PlanFrame step at a time and logs
  it as a MakeStep (`make` skill).
- `plugins/pmcro-checker/` — evaluates MakeStep results against the
  PlanFrame's success_criteria (`check` skill).
- `plugins/pmcro-reflector/` — closes a cycle: records a disposition,
  optionally files a next seed, seals the trail (`reflect-and-seed`
  skill). Sole role permitted to seal.
- `.claude/skills/` — repo-authoring tooling for whoever (human or agent)
  works on this repo itself: `create-plugin` (packaging a plugin correctly
  for Cowork upload) and `run-pmcro-cycle` (the mint→log→seal→commit
  procedure). These are not colony products — they document how to build
  this repo, not a capability a PMCR-O role invokes mid-cycle.
- `.pmcro/trails/` — sealed cycle evidence for this repo's own build
  history (dogfooded: this repo's own scaffolding is itself being run as a
  PMCR-O cycle, Class B, one frame file per phase, each line
  schema-conformant).
- `.pmcro/queue/` — seed-intent inbox. Items filed here (by Reflector, or
  by direct human intent capture) conform to
  `plugins/pmcro-reflector/skills/reflect-and-seed/assets/schema.queue-item.asset.json`.

## Conventions

- No absolute, host-specific, or drive-letter paths in system-authored
  content (frames, docs, configs). Reference files repo-relative.
- Trail-as-Product: trail frames are instances of a declared schema
  (`plugins/pmcro-trail/skills/initialize/assets/schema.trail-frame.asset.json`),
  not hand-typed prose. The same discipline extends to queue items (see
  above) — any durable, structured artifact this colony produces gets a
  declared schema, not ad hoc shape.
- Every phase file (`orchestrate.jsonl` / `plan.jsonl` / `make.jsonl` /
  `check.jsonl` / `reflect.jsonl`) has exactly one owning role plugin;
  nothing else appends to it.
- Log incrementally, as work happens, not as a dump at the end of a cycle.

## Not here yet (intentionally out of scope for now)

A `.agents/` authoring surface, laws/policies/capabilities layers, and
automated dispatch (today, a session runs a cycle by following each role's
`run.<name>.asset.md` in turn — there is no scheduler). These land as their
own seeds, not invented ahead of being asked for.
