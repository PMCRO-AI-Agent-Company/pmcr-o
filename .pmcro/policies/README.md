# policies

Prose-adjacent, still-structured YAML policy: who (which role) may do
what, what's allowed on the network, and the security/trust posture. Laws
(`../laws/`) are the fixed rule IDs; these are the configurable policy
that implements them.

| File | Covers |
|---|---|
| `permissions.yaml` | Per-role may/mayNot — directly maps onto this repo's five roles |
| `execution.yaml` | Capability-level allow/policy/deny defaults, evidence minimums |
| `network.yaml` | Network default-deny posture, browser/container network rules |
| `security.yaml` | Trust posture for external input, secrets handling, approval gates |

## Source and adaptation

Ported from `pmcro-skills_archive` `.pmcro/policies/` @ `main` (commit
`d864f70`). `permissions.yaml` and `security.yaml` were kept essentially
verbatim — both are already stack-neutral (role names and trust/approval
posture, no tool names). `execution.yaml` and `network.yaml` keep their
schema and per-capability shape, but every capability they mention
(`github`, `browser`, `containers`, `mcp-gateway`, `host-command-execution`,
`filesystem`) is **not wired to a real provider in this repo** — see
`../capabilities/README.md` and `../providers/README.md`. The per-capability
policy stays as a template for when a capability is actually added, with
an explicit comment saying so, rather than being silently dropped (losing
the pattern) or silently kept as if it were live (a false claim).
`execution.yaml`'s `environmentOverride` note, which assumed an
`environments.yaml` split, was dropped — see `../config/README.md` for
why this repo doesn't have one yet.
