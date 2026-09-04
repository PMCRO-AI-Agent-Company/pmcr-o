# MAF Declarative Workflow / Harness / CodeAct -- Migration Proposal for PMCR-O

**Status:** PROPOSAL ONLY. Not approved. No runtime behavior in this repo
changes as a result of this document. Written at explicit user request to
scope options before any implementation begins.

**Scope of this document:** whether, and how much of, the Microsoft Agent
Framework (MAF) declarative-workflow / Harness / CodeAct architecture that
the sibling `pmcro-runtime` project has been building should be brought into
`P:\ProjectName` (this repo, `pmcr-o`) -- the actual governed PMCR-O colony.

---

## 1. Why this document exists

This repo (`pmcr-o`) and `pmcro-runtime` are two different things that share
a name and a governance vocabulary:

- **`pmcr-o`** (this repo) is the actual governed colony: deterministic
  PowerShell phase scripts, file-backed queue/trail/state, plugin-based
  agent skills, evidence sealed under `.pmcro/trails/`. It runs today.
- **`pmcro-runtime`** is a separate, still-unproven engineering effort to
  make PMCR-O's Plan-Make-Check-Reflect cycle be *executed* by a Microsoft
  Agent Framework declarative workflow (YAML + PowerFx) instead of hand-code,
  with local Ollama-backed agents instead of Azure Foundry.

The user asked, after reviewing `pmcro-runtime`'s state, to propose a design
for bringing that architecture (or parts of it) into `pmcr-o` itself --
explicitly design-first, not implementation-first.

## 2. Current state of `pmcr-o` (this repo)

- Cycle mechanics are deterministic PowerShell scripts, one per phase, under
  `plugins/pmcro-{orchestrator,planner,maker,checker,reflector}/skills/*/scripts/*.ps1`.
  Each script appends one governed frame (OrchestrateFrame, PlanFrame,
  MakeStep, CheckFrame, Reflection) to a trail. No LLM sits inside these
  scripts -- the reasoning happens in the Claude session driving them; the
  scripts are the deterministic evidence-writing mechanism.
- `.pmcro/AGENTS.md` invariants: evidence immutability, "never seal a trail
  outside the Reflector role and Checker gate," deterministic mechanics vs.
  domain reasoning kept separate, Atomic File Protocol (no partial
  edits/TODOs left mid-mutation).
- Plugin layout: `plugin.json` + `.claude-plugin/plugin.json` +
  `agents/<role>.md` + `skills/<skill>/SKILL.md` + `assets/` + `references/`
  -- an open-spec progressive-disclosure shape that, per this session's
  research, is structurally the same thing MAF's own Agent Skills spec
  (agentskills.io) formalizes.
- Current state: queue empty (8/8 done), 27 sealed trails, colony IDLE. No
  active work in flight.

## 3. What `pmcro-runtime` has actually proven (evidence, not aspiration)

Checked directly against source on 2026-09-04 (`P:\source\pmcro-runtime`,
commit `c86cc03`, the more-current of two diverging local copies):

- **The `ResponseAgentProvider` blocker is solved, locally.** MAF declarative
  workflows can only invoke agents through a `ResponseAgentProvider`; the
  only built-in implementation (`AzureAgentProvider`) requires an Azure AI
  Foundry project. `pmcro-runtime` wrote its own
  `SubjectAgentRegistryProvider : ResponseAgentProvider` that bridges to
  local Ollama-backed `ChatClientAgent`s instead. This directly answers the
  "I'll be using the best local model fit for my computer, not Azure"
  requirement -- the pattern to reuse, if any of this is adopted, is this
  custom provider, not `AzureAgentProvider`.
- **CodeAct is live, not a stub.** A registered `codeact-agent`
  (`ARCH-CODEACT-001`, 2026-07-12) runs Python inside a
  `HyperlightCodeActProvider` sandbox, deliberately scoped to read-only
  tools only (approval is per `execute_code` call, not per tool call inside
  it, so a mutating tool would let one approval authorize unbounded writes).
  It now doubles as the execution surface for the nine C-Suite "chief"
  domains (CEO/CFO/CTO/etc.), each loading its own skill via `load_skill`
  before acting.
- **A Harness-shaped skill provider exists** (`AgentSkillsProvider`,
  `load_skill`/`read_skill_resource`), reused across the Orchestrator,
  Harness, and codeact agents. Not yet confirmed whether this is literally
  built on the `Microsoft.Agents.AI.Harness` package or is a hand-rolled
  equivalent -- open question, not yet resolved.
- **CopilotKit/Next.js is documented as the UI/transport layer** in
  `PMCR-O-DECLARATIVE-ARCHITECTURE.md`'s layer-ownership table, above the
  Orchestrator API. `pmcr-o` has no UI layer today -- N/A unless this repo
  grows one.

## 4. What `pmcro-runtime` has NOT proven yet (by its own admission)

This matters as much as section 3 -- these are the project's own documented
gaps, not my speculation:

- `pattern-a-macro-cycle.yaml`'s own header comment says **"NOT YET
  LOAD-BEARING."**
- `.migration-status.md` lists 10 validation gates before the legacy
  `PmcroLoop` can be removed; as of the last check, gates 3-5 and 6 were
  still open.
- A real, documented SDK-level bug: PowerFx's `Find()` function is unreliable
  against `System.LastMessage`/`AgentResponse`-sourced text in the current
  preview build of `Microsoft.Agents.AI.Workflows.Declarative`
  (`ARCH-DECLARATIVE-013/015/016`). This breaks BOTH the TYPE1
  human-in-the-loop pending-detection AND the RETRY-signal detection in the
  workflow's own loop-control `If` condition -- i.e. the two places where
  the workflow decides "stop and ask a human" or "try again" are both built
  on a function known not to work reliably.
- The "Critical evidence invariant" the project's own architecture doc
  states -- `MCP success -> normalized execution artifact -> Checker input
  -> CheckItem coverage -> Gate decision` -- is explicitly named as NOT yet
  satisfied end to end (the "Gate 3 evidence-coverage defect" the migration
  constraint references).
- Two local checkouts had diverged with uncommitted work and a pending seed
  explicitly blocking on a concurrent session, last time this was checked.

None of this means the approach is wrong. It means: nothing here is a
proven, drop-in dependency yet. Anything brought into `pmcr-o` from
`pmcro-runtime` today would be importing an experiment, not a library.

## 5. Proposed options (graduated, not all-or-nothing)

### Option A -- CodeAct pilot on one bounded plugin step (lowest risk)

Find one existing PMCR-O plugin step (in any of the six cycle-phase plugins,
or a future one) that currently does several small, chainable tool calls in
sequence -- the exact shape CodeAct is meant for. Add a
`Microsoft.Agents.AI.LocalCodeAct` or Hyperlight-backed `execute_code` path
for just that step, run it as a real governed trail cycle, and compare
tool-call count / latency against the existing approach with actual numbers.
No change to the deterministic phase-script mechanism itself; this is purely
about what a Maker step is allowed to do internally.

**Risk:** low. Additive, reversible, touches one step. **Unlocks:** a real,
measured answer to "is CodeAct worth it here" instead of a guess.

### Option B -- Harness-style skill provider for existing plugin skills

Replace or wrap the current SKILL.md-reading convention with an actual
`AgentSkillsProvider`-equivalent (`load_skill`/`read_skill_resource` as real
callable tools) for one or two of the existing six plugins, so an agent
loads a skill on demand instead of the whole thing being front-loaded into
context.

**Risk:** medium. Touches how every plugin's skill content reaches an agent.
**Open question:** does `pmcr-o` currently front-load skill content in a way
that actually costs anything, or is this solving a problem that doesn't
exist here yet? Needs a real measurement before committing, not just "MAF
does it this way."

### Option C -- Full MAF declarative workflow replacing PowerShell phase scripts

Mirror what `pmcro-runtime` is attempting: a YAML-defined workflow owning
Plan-Make-Check-Reflect sequencing, PowerFx branching, `SubjectAgentRegistryProvider`
(or equivalent) bridging to local Ollama agents.

**Risk:** high, and not just technically. This would mean adopting an
architecture that its own originating project has not yet cleared its own
validation gates for, inside the repo that's actually governed and in
active use -- the reverse of the usual dependency direction (mature ->
adopter). Section 4's SDK bug alone (`Find()` breaking both HIL and retry
detection) would land directly in this repo's Checker-gate and
trail-sealing path if copied as-is today.

## 6. What must not change regardless of which option is chosen

- Evidence immutability (`.pmcro/AGENTS.md`): sealed trails are never
  retroactively edited.
- "Never seal a trail outside the Reflector role and Checker gate" -- a MAF
  workflow, if ever adopted, would need to call into this repo's existing
  seal mechanism, not invent its own (this is exactly the discipline
  `pmcro-runtime`'s `DeclarativeCycleRunner` already follows -- it calls the
  same `ITrailWriter`/`LoopFrameBuilders` the hand-rolled path uses, so the
  two produce byte-for-byte compatible evidence. That specific discipline is
  worth copying even if the surrounding workflow isn't, yet).
- Atomic File Protocol (no partial edits/TODOs left mid-mutation).
- The queue/state/trail mechanics stay file-backed and deterministic for
  now -- none of the three options above touch that layer.

## 7. Risk register (summary)

| Risk | Where it bites | Mitigation |
|---|---|---|
| PowerFx `Find()` SDK bug | Option C only | Don't adopt Option C until `pmcro-runtime` itself has a fix or workaround proven, not just documented as known |
| Importing an unproven dependency into a governed, in-use repo | Option C | Graduated rollout (A before B before C), each gated on its own measured evidence, exactly as PMCR-O's own Checker-gate philosophy already demands |
| Solving a problem that doesn't exist here yet | Option B especially | Measure `pmcr-o`'s actual current skill-loading cost before replacing the mechanism |
| Scope creep from "design proposal" into silent implementation | All | This document is deliberately not accompanied by any code change; next step requires an explicit choice |

## 8. Recommendation

Start with **Option A** only, as a single bounded pilot trail. Do not begin
Option B or C until Option A produces real, measured evidence one way or
the other -- consistent with this repo's own evidence-over-assertion
philosophy (`trail-as-product.md`, `capability-gap-and-composition.md`).

## 9. Open decisions needed from the user

1. Approve Option A (and if so, which existing plugin step to pilot it on),
   Option B, Option C, or none of the above yet.
2. Whether to wait for `pmcro-runtime` to clear its own Gate 3 (evidence
   coverage) and the `Find()` bug before importing *any* piece of it, even
   under Option A.
3. Whether CopilotKit/Next.js is in scope for `pmcr-o` at all -- there is
   currently no UI layer in this repo, so this piece of the reference
   architecture may simply not apply here.

No `next_seed` is filed by this trail: which option to pursue is a decision
for the user, not something to auto-queue.
