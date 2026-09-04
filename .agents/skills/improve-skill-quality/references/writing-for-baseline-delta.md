# Writing for Delta Over the Baseline

Every skill in this repository is judged against the same model running
with **no skill loaded** — the baseline arm in `eval.yaml`. This is the
single fact that should shape every section of a `SKILL.md`.

## Why this matters

A capable model already knows a lot. If a skill's content is something
the model would produce unaided, that content scores zero — it costs
context and reading time for no behavior change. If the content makes
the model slower, more hedged, or more likely to ask an unnecessary
clarifying question, it scores **below** zero: the skill made the model
worse than having no skill at all.

## What actually earns a delta

- **A decision the model would otherwise get wrong.** Not "explain what
  a build failure is" but "when the build fails with error X, do Y and
  never Z."
- **A concrete output contract.** An exact command, a verdict line
  format, a findings table shape — not "consider checking the logs."
- **Stop-conditions.** Rules that prevent the model from over-applying
  a fix, rewriting working code, or acting before it has verified
  something.
- **Failure honesty instructions.** Telling the model to report a real
  restore/build/test failure rather than imply success on partial work.
- **Discovery over hardcoding.** Telling the model *how* to find a repo
  path beats listing the path as a required input the user must supply.

## What does not earn a delta

- Restating a well-known API signature, CLI flag list, or concept the
  model already reproduces correctly from training.
- A 12-section template applied to an 8-line task, padding the skill to
  "look complete" rather than matching the task's actual complexity.
- Advice phrased as options ("you may want to…") instead of a rule
  ("do this, never that").
- Content that duplicates what another `SKILL.md` or `*.agent.md` in
  this repo already owns — reference it instead of restating it.

## How to tell which bucket a section is in

Before keeping a section, ask: if I ran the exact same prompt against
this model with the skill unloaded, would it already do this correctly?
If yes, cut the section. If the honest answer is "usually, but it
sometimes skips a step" — that's the actual delta; write the rule that
prevents the skip, not a restatement of the whole procedure.
