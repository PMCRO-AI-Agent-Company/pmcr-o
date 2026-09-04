# PMCR-O Laws

1. **L-DISPATCH** — no cycle or subagent runs without explicit Orchestrator routing and trail binding.
2. **L-ATOMIC-OUTPUT (EC-SYS-001)** — file writes must be complete and unfragmented.
3. **L-MINIMALIST-PLANNING (EC-SYS-002)** — PlanFrames scope only executable, validatable one-cycle work.
4. **L-CHECKER-GATE** — no sealed/done trail without PASS in `check.jsonl`.
5. **L-TRAIL-CLASS-B** — every cycle records structured single-line JSON events across `orchestrate.jsonl`, `plan.jsonl`, `make.jsonl`, `check.jsonl`, `reflect.jsonl`, and `disposition.json`.
6. **L-SEED-IMMUTABILITY** — raw `messy_seed` is immutable once enqueued; refinement belongs in `canonical_seed`.
7. **L-NO-DRIVE-LETTERS** — durable paths use forward slashes or relative workspace notation; hardcoded drive letters are prohibited.

These repository laws complement, rather than silently replace, the machine-readable laws in `.pmcro/laws/laws.yaml`.
