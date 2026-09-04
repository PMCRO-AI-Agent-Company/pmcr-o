# PMCR-O State

- Active Trail ID: null
- Current Task ID: null
- Active Cycle: 0
- Status: IDLE
- Lease Holder: null
- Lease Expiry: null

## Reconciled runtime counts

- pending queue items: 0
- completed queue items: 8 (task-hand-run-full-five-plugin-cycle, task-add-trail-link-path-script, task-rebuild-orchestrationapi-agent-service, task-build-pmcro-aspire-mcp-skill, task-add-orchestrationapi-grpc-integration-test, task-verify-pmcro-aspire-live-connection, task-wire-pmcro-env-vars-into-programs, task-adopt-aspire-cli-lifecycle-session-wide)
- sealed trails: 27
- last sealed trail: `16bc79c3-ede5-4680-b32e-f273cdae48b7` (task-port-earned-knowledge-schema-from-legacy-repo, ad hoc -- not from the queue)

## Lock table

- queue: SHARED

## Reconciliation rule

This state document is reconciled from the durable queue and trail evidence. It is descriptive state, not permission to invent work. Interrupted claims remain discoverable in `queue/pending/` and require lease/trail verification before recovery.
