# PMCR-O Package Context Budget

## Principle

A package must adapt to the consumer's effective context budget. Do not assume that the model's advertised context window equals the safe size of one prompt, turn, tool result, or skill activation.

## Decision order

When the selected material fits the effective budget:

```text
full fidelity → one package
```

When it does not fit:

```text
full fidelity
    ↓
priority selection
    ↓
chunking
    ↓
multiple ordered text packages
```

Only add summarization as a deliberate derived artifact when the task permits loss of detail.

## Fidelity rules

Source code, `SKILL.md`, configuration, scripts, and references should remain lossless whenever they are included as authoritative package content. Compression by paraphrasing is not a substitute for the source.

A summary may be emitted alongside source material as navigation/context, but it must be labeled as derived and must never replace the canonical file contents silently.

ZIP compression reduces storage/transfer size but does not reduce the token count after files are read by an LLM. For text-only delivery, token-aware selection or chunking is therefore the relevant control.

## Budget inputs

A target adapter may provide:

- `max_input_tokens` — maximum input tokens for the request;
- `reserved_output_tokens` — output budget that must remain available;
- `context_reserve_tokens` — safety allowance for system instructions, conversation, tools, and other runtime material;
- `max_package_tokens` — preferred size of one generated package/chunk.

The usable package budget is approximately:

```text
usable = max_input_tokens
       - reserved_output_tokens
       - context_reserve_tokens
```

The implementation should keep a safety margin rather than filling the limit exactly.

## Unknown budgets

When the target's effective budget is unknown, prefer smaller deterministic chunks over one monolithic text artifact. Record that the budget was unknown in the package manifest.

## Chunking

Chunks must:

1. have stable sequence numbers;
2. preserve complete file boundaries whenever practical;
3. never split `SKILL.md` front matter from its body;
4. preserve repository-relative paths;
5. include the package/revision identifier;
6. state whether the chunk is complete or continued;
7. include a manifest that maps files to chunks.

Example:

```text
PMCR-O PACKAGE
FORMAT: PMCR-O-TXT-PACK/1
CHUNK: 1/4
...
```

## Adaptive selection

Priority should normally be:

```text
current Seed Intent / Goal
→ required SKILL.md
→ directly referenced resources
→ required scripts
→ required assets/templates
→ supporting references
→ broader repository context
```

The Orchestrator may change this ordering based on the task and Checker evidence.

## Runtime behavior

A package planner should detect when a requested full export cannot fit and change the packaging strategy instead of blindly retrying. The O-Mode decision should be recorded when the strategy changes, for example:

```text
STRATEGY: full-context
RESULT: exceeds effective input budget
NEXT STRATEGY: priority-chunked-context
```

## Safety

A smaller package must not be described as complete unless all selected content is present. A partial or summarized package must carry explicit completeness metadata.
