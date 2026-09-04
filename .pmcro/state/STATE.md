# PMCR-O State

- Active Trail ID: null
- Current Task ID: null
- Active Cycle: 0
- Status: IDLE
- Lease Holder: null
- Lease Expiry: null

## Reconciled runtime counts

- pending queue items: 1 (task-add-trail-link-path-script)
- completed queue items: 1 (task-hand-run-full-five-plugin-cycle)
- sealed trails: 17
- last sealed trail: `4e2a7d63-44df-429a-9c9d-39baa00ce190`

## Lock table

- queue: SHARED

## Reconciliation rule

This state document is reconciled from the durable queue and trail evidence. It is descriptive state, not permission to invent work. Interrupted claims remain discoverable in `queue/pending/` and require lease/trail verification before recovery.
