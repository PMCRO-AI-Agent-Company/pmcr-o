# PMCR-O State

- Active Trail ID: null
- Current Task ID: null
- Active Cycle: 0
- Status: IDLE
- Lease Holder: null
- Lease Expiry: null

## Reconciled runtime counts

- pending queue items: 2 (task-build-pmcro-aspire-mcp-skill, task-rebuild-orchestrationapi-agent-service)
- completed queue items: 2 (task-hand-run-full-five-plugin-cycle, task-add-trail-link-path-script)
- sealed trails: 20
- last sealed trail: `59143093-6276-45c1-b25e-bd4134d0f30a`

## Lock table

- queue: SHARED

## Reconciliation rule

This state document is reconciled from the durable queue and trail evidence. It is descriptive state, not permission to invent work. Interrupted claims remain discoverable in `queue/pending/` and require lease/trail verification before recovery.