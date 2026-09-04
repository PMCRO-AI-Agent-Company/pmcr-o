# runtime

Runtime-level policy and contracts: the config toggles every cycle should
respect, the governed-output envelope, and short notes on the
Orchestrator/Harness/durability concepts.

| File | Covers |
|---|---|
| `config.yaml` | Policy toggles: evidence/checker/reflection required, queue claim policy |
| `output-contract.md` | The governed-result envelope every completion should satisfy |
| `validate_output_contract.py` | Deterministic validator for that envelope |
| `orchestrator/ORCHESTRATOR.md` | Orchestrator role notes (see file for how this differs from the real plugin) |
| `harness/HARNESS.md` | What an autonomous multi-cycle harness would be (this repo doesn't have one) |
| `durability/BEYOND-TOKEN-LIMIT.md` | Why externalizing state to git + trails + queue is the durability strategy |

## `validate_output_contract.py` — tested for real

Before being trusted enough to port (per this repo's own "never trust a
script without running it" convention, `README.md` at the repo root), this
script was copied to a scratch location and run against 6 cases:

| Case | Input | Expected | Got |
|---|---|---|---|
| 1 | Only `{"action":"COMPLETE"}` | `FAIL`, missing-field errors for all 6 required fields, plus completion-requires-evidence/checker | Matched, exit 1 |
| 2 | Full envelope, `action:COMPLETE`, no evidence/checker | `FAIL`, 2 completion errors | Matched, exit 1 |
| 3 | Full envelope, `action:COMPLETE`, evidence present, `checker.status:PASS` | `PASS` | Matched, exit 0 |
| 4 | Full envelope, non-completion `action:ROUTE` | `PASS` (completion checks don't apply) | Matched, exit 0 |
| 5 | No arguments | Usage message, exit 2 | Matched |
| 6 | `required_evidence` given as a string, not an array | `FAIL`, type error | Matched, exit 1 |

All six matched expected behavior exactly. The script is pure stdlib
(`json`, `sys`, `pathlib`) with no MAF/Aspire/Docker dependency, and its
`SCHEMA_PATH` variable is assigned but never read by the validation logic
— it runs correctly with no `output-contract.schema.json` present, which
matters here since that file doesn't exist in either repo (see
`output-contract.md`'s own "Source and adaptation" note).

See `../README.md` for what else was and wasn't ported into `.pmcro/`.
