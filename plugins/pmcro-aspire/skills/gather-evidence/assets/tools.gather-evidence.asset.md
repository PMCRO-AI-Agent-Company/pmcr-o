# Tool catalog -- pmcro-aspire:gather-evidence

Source: trail `ba0c2c65-075f-470b-abed-e5647053dc8b`, MakeStep 2 (fetched
https://aspire.dev/get-started/aspire-mcp-server/, 2026-09-04). 14 tools,
grouped by what they're useful for as PMCR-O evidence:

## Resource state

- `list_apphosts` -- which AppHosts the server can see.
- `select_apphost` -- pick one when more than one is visible.
- `list_resources` -- names, types, states, health, endpoints for every
  resource in the selected AppHost. The primary tool for "is
  projectname-runtime / projectname-orchestrationapi / ollama-server
  actually Running" style CheckFrame criteria.

## Telemetry

- `list_console_logs` -- raw stdout/stderr for a resource. Use to confirm a
  claimed runtime behavior (e.g. a logged exception, a specific log line)
  actually happened, not just that the process is up.
- `list_structured_logs` -- structured/OTel log records.
- `list_traces` -- distributed traces, useful for confirming a cross-service
  call (e.g. OrchestrationApi's gRPC call into Runtime) actually completed
  and where time was spent.

## Control (narrow)

- `execute_resource_command` -- runs one of the resource-scoped commands
  Aspire itself defines for that resource type (e.g. restart). Not a
  general command-execution tool; it cannot run arbitrary shell commands
  and is not a substitute for this repo's own Maker role.

## Docs lookup

- `list_docs`, `search_docs`, `get_doc`, `get_integration_docs` -- Aspire's
  own documentation, searchable. Useful for a Maker step that needs current
  Aspire API/behavior confirmation instead of relying on training data.

## Meta

- `doctor` -- self-diagnostic; confirms the server can see a running
  AppHost. Always call this first (see the setup asset).
- `refresh_tools` -- re-fetches the tool list from a running AppHost (some
  tools may be app-specific/dynamic).

## Explicitly excluded

Per Aspire's own docs, this server does not expose source code, secrets, or
environment variable values, by design. Never treat its silence on any of
those as evidence one way or the other -- it simply does not answer.
