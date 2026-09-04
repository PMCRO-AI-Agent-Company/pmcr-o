# PMCR-O State

- Active Trail ID: null
- Current Task ID: null
- Active Cycle: 0
- Status: IDLE
- Lease Holder: null
- Lease Expiry: null

## Reconciled runtime counts

- pending queue items: 2 (task-build-pmcro-aspire-mcp-skill, task-add-orchestrationapi-grpc-integration-test)
- completed queue items: 3 (task-hand-run-full-five-plugin-cycle, task-add-trail-link-path-script, task-rebuild-orchestrationapi-agent-service)
- sealed trails: 21
- last sealed trail: `eb6c47d4-817e-4004-bd76-3242cce889ef`

## Lock table

- queue: SHARED

## Reconciliation rule

This state document is reconciled from the durable queue and trail evidence. It is descriptive state, not permission to invent work. Interrupted claims remain discoverable in `queue/pending/` and require lease/trail verification before recovery.
