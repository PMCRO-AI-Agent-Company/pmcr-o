# PMCR-O State

- Active Trail ID: null
- Current Task ID: null
- Active Cycle: 0
- Status: IDLE
- Lease Holder: null
- Lease Expiry: null

## Reconciled runtime counts

- pending queue items: 1 (task-adopt-aspire-cli-lifecycle-session-wide)
- completed queue items: 7 (task-hand-run-full-five-plugin-cycle, task-add-trail-link-path-script, task-rebuild-orchestrationapi-agent-service, task-build-pmcro-aspire-mcp-skill, task-add-orchestrationapi-grpc-integration-test, task-verify-pmcro-aspire-live-connection, task-wire-pmcro-env-vars-into-programs)
- sealed trails: 25
- last sealed trail: `b6299719-6309-4df2-a208-83e891f8d4ad`

## Lock table

- queue: SHARED

## Reconciliation rule

This state document is reconciled from the durable queue and trail evidence. It is descriptive state, not permission to invent work. Interrupted claims remain discoverable in `queue/pending/` and require lease/trail verification before recovery.
