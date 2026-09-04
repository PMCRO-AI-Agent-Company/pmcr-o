# PMCR-O State

- Active Trail ID: null
- Current Task ID: null
- Active Cycle: 0
- Status: IDLE
- Lease Holder: null
- Lease Expiry: null

## Reconciled runtime counts

- pending queue items: 2 (task-add-orchestrationapi-grpc-integration-test, task-verify-pmcro-aspire-live-connection)
- completed queue items: 4 (task-hand-run-full-five-plugin-cycle, task-add-trail-link-path-script, task-rebuild-orchestrationapi-agent-service, task-build-pmcro-aspire-mcp-skill)
- sealed trails: 22
- last sealed trail: `3a9ec041-f2a4-4e06-864f-a8f723201122`

## Lock table

- queue: SHARED

## Reconciliation rule

This state document is reconciled from the durable queue and trail evidence. It is descriptive state, not permission to invent work. Interrupted claims remain discoverable in `queue/pending/` and require lease/trail verification before recovery.
