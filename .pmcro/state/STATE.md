# PMCR-O State

- Active Trail ID: null
- Current Task ID: null
- Active Cycle: 0
- Status: IDLE
- Lease Holder: null
- Lease Expiry: null

## Reconciled runtime counts

- pending queue items: 0
- completed queue items: 2 (task-hand-run-full-five-plugin-cycle, task-add-trail-link-path-script)
- sealed trails: 18
- last sealed trail: `cff63451-185c-4a61-968a-cc126f5f1481`

## Lock table

- queue: SHARED

## Reconciliation rule

This state document is reconciled from the durable queue and trail evidence. It is descriptive state, not permission to invent work. Interrupted claims remain discoverable in `queue/pending/` and require lease/trail verification before recovery.
