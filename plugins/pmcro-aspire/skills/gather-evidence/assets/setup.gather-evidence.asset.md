# Setup -- pmcro-aspire:gather-evidence

The Aspire MCP server ships with the Aspire CLI; it is not a separate
install. It runs as a local STDIO child process (no open network port), so
there is no auth to configure.

## One-time registration

From a working directory where an AppHost project (or an
`aspire.config.json` pointing at one -- this repo's
`ProjectName.AppHost/aspire.config.json` already does) can be resolved:

```
aspire agent init
```

This generates client MCP config for the agent(s) it recognizes on the
machine (Aspire's docs name VS Code, Claude Code, GitHub Copilot CLI, and
OpenCode). Per Aspire's own docs the server is not restricted to those
four -- any MCP-capable STDIO client can add it. For a client not covered
by `aspire agent init`, register it manually as an MCP server whose command
is:

```
aspire agent mcp
```

run from the repo root (or anywhere `aspire.config.json` / the AppHost
project can be resolved from).

## Verifying the connection

Once registered, call the `doctor` tool first. It self-reports whether the
server can see a running AppHost. If it reports no AppHost found, the
target AppHost (`ProjectName.AppHost`) must actually be running
(`dotnet run --project ProjectName.AppHost`, or the Aspire CLI equivalent)
-- this skill observes a running AppHost, it does not start one.

## Known limitation as of this writing

This skill has been scoped and documented from Aspire's own current docs,
but `aspire agent init` has not yet been run against this repo's own agent
runtime -- there is no live registered connection to exercise yet. Treat
`gather-evidence` as scoped-and-documented, not yet live-verified; the
first real use should record whether `doctor` succeeds as its own MakeStep
evidence, and any tool-shape drift found against the catalog below should
be corrected here rather than worked around silently.
