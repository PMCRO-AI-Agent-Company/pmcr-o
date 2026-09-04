# PMCR-O State

- Active Trail ID: null
- Current Task ID: null
- Active Cycle: 0
- Status: IDLE
- Lease Holder: null
- Lease Expiry: null

## Reconciled runtime counts

- pending queue items: 2 (task-wire-pmcro-env-vars-into-programs, task-adopt-aspire-cli-lifecycle-session-wide)
- completed queue items: 6 (task-hand-run-full-five-plugin-cycle, task-add-trail-link-path-script, task-rebuild-orchestrationapi-agent-service, task-build-pmcro-aspire-mcp-skill, task-add-orchestrationapi-grpc-integration-test, task-verify-pmcro-aspire-live-connection)
- sealed trails: 24
- last sealed trail: `1de7efcb-bff0-4e7f-a9ae-7754399a0971`

## Lock table

- queue: SHARED

## Reconciliation rule

This state document is reconciled from the durable queue and trail evidence. It is descriptive state, not permission to invent work. Interrupted claims remain discoverable in `queue/pending/` and require lease/trail verification before recovery.
