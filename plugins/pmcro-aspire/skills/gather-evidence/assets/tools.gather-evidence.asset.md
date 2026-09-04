# Command catalog -- pmcro-aspire:gather-evidence

Corrected in trail `d360b692-5014-4267-9018-9b94758e9170` (2026-09-04) --
the version 0.1.0 asset documented a 14-tool MCP server catalog
(`list_resources`, `list_console_logs`, ...) sourced from
https://aspire.dev/get-started/aspire-mcp-server/ without ever running it
against this repo. When actually exercised, `aspire agent init` did not
register that MCP server at all -- the real, working mechanism is the
Aspire CLI directly, via the local backchannel socket
(`~/.aspire/backchannels/`), documented by the `aspire`, `aspire-monitoring`,
and `aspire-orchestration` skill files `aspire agent init` installs into
`.agents/skills/`. Read those directly for the authoritative, versioned
detail -- this file is a PMCR-O-evidence-focused summary of them, not a
replacement.

## Resource state

- `aspire ps --format Json` -- lists running AppHosts (path, PID, dashboard
  URL). AppHost-level, not per-resource.
- `aspire describe --format Json` -- full per-resource state: name, type,
  `state` (e.g. `Running`), `healthStatus` (e.g. `Healthy`), URLs,
  environment variables, relationships, and available commands. The
  primary tool for "is projectname-runtime / projectname-orchestrationapi
  / ollama-server / model-orchestrator actually Running and Healthy" style
  CheckFrame criteria. Add `--include-hidden` for proxies/helper
  containers/migrations that are filtered out by default.
- `aspire wait <resource> --non-interactive` -- blocks until a named
  resource reports healthy; use this instead of an HTTP polling loop.

## Telemetry

- `aspire logs <resource>` (add `--follow` to stream) -- raw stdout/stderr.
  Use to confirm a claimed runtime behavior actually happened, not just
  that the process is up.
- `aspire otel logs [resource]` -- structured/OTel log records.
- `aspire otel traces [resource]` -- distributed traces, useful for
  confirming a cross-service call (e.g. OrchestrationApi's gRPC call into
  Runtime) actually completed and where time was spent.
- `aspire otel spans [resource]` -- individual span detail.
- `aspire export` -- a portable zipped telemetry snapshot for deeper
  offline analysis.

## Lifecycle (see the setup asset for the full mandatory-compliance rule)

- `aspire start --non-interactive` / `aspire stop --non-interactive` --
  start/stop the AppHost cleanly. Never `dotnet run --project <AppHost>`
  plus manual process killing -- that is the documented cause of
  `MSB3027`/`MSB3021`/`MSB3491` file-lock errors
  ([microsoft/aspire#15801](https://github.com/microsoft/aspire/issues/15801)).
- `aspire resource <name> <command>` -- runs one of the commands a
  resource itself exposes (visible in `aspire describe`'s `commands`
  field, e.g. `restart`, `stop`, or a project's `rebuild`). Not a general
  command-execution tool.

## Docs lookup

- `aspire docs search <topic>` / `aspire docs api search <query>
  --language csharp|typescript` -- Aspire's own current documentation and
  API reference, searchable. Useful for a Maker step that needs current
  Aspire API/behavior confirmation instead of relying on training data.

## Diagnostics

- `aspire doctor` -- environment diagnostic (SDK, CLI, Docker, etc.), not
  an AppHost-state check.

## Explicitly out of reach

The Aspire CLI's backchannel only talks to a locally running AppHost by
design -- there is no remote backchannel. For a deployed app, or a
standalone dashboard, route to `aspire-deployment` / `aspire-monitoring`'s
own documented paths (`azure-diagnostics`, `kubectl`, `docker logs`,
`--dashboard-url`) instead of assuming these commands reach it. Reading
source code, secrets, or environment *values* is not something these
commands are for either, even though `aspire describe`'s `environment`
field does surface variable names and (non-secret) values set by the
AppHost -- do not paste secret-bearing environment values into trail
evidence even when a command happens to return them.
