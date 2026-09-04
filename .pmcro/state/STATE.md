# PMCR-O State

- Active Trail ID: null
- Current Task ID: null
- Active Cycle: 0
- Status: IDLE
- Lease Holder: null
- Lease Expiry: null

## Reconciled runtime counts

- pending queue items: 2 (task-verify-pmcro-aspire-live-connection, task-wire-pmcro-env-vars-into-programs)
- completed queue items: 5 (task-hand-run-full-five-plugin-cycle, task-add-trail-link-path-script, task-rebuild-orchestrationapi-agent-service, task-build-pmcro-aspire-mcp-skill, task-add-orchestrationapi-grpc-integration-test)
- sealed trails: 23
- last sealed trail: `d360b692-5014-4267-9018-9b94758e9170`

## Lock table

- queue: SHARED

## Reconciliation rule

This state document is reconciled from the durable queue and trail evidence. It is descriptive state, not permission to invent work. Interrupted claims remain discoverable in `queue/pending/` and require lease/trail verification before recovery.
