# secrets

References and policy only. This directory must never contain secret
values, tokens, passwords, private keys, certificates, connection-string
credentials, `.env` files, or provider exports.

## Reference contract

| Concern | PMCRO record | Runtime owner |
| --- | --- | --- |
| A provider's auth token | Parameter reference and least-privilege scope, in `../config/parameters.yaml` | Whatever secret store the runtime environment provides |
| CI/CD secrets | Named parameter reference | CI's native secret store, injected at run time |

This repo currently has no live provider that needs a credential (see
`../providers/README.md`), so the table above is a template, not a
record of anything real yet.

## Rotation and evidence

- Rotate values at the external secret provider; update only the
  reference or policy here when the contract changes.
- Never attach secret values to trails, frames, evidence, logs,
  telemetry, prompts, memory, or generated artifacts.
- Record only redacted provider identifiers and the outcome of
  configuration validation.

## Source and adaptation

Ported from `pmcro-skills_archive` `.pmcro/secrets/README.md` @ `main`
(commit `d864f70`). The "reference, never value" contract and the
rotation/evidence rules are stack-neutral and kept as-is. The reference
table's row names were genericized — the archive's rows named GitHub MCP,
Docker MCP Toolkit, and Azure Key Vault specifically; none of those exist
here yet.
