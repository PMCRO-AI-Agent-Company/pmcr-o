# Setup -- pmcro-aspire:gather-evidence

Corrected in trail `d360b692-5014-4267-9018-9b94758e9170`'s live
verification (2026-09-04) after the version 0.1.0 setup asset -- based on
reading https://aspire.dev/get-started/aspire-mcp-server/ in trail
`ba0c2c65` without exercising it -- turned out to describe a different,
unused integration path. What follows is what actually happened when this
was run for real.

## What `aspire agent init` actually does

Running `aspire agent init --non-interactive` from the repo root:

- Detects installed agent environments on the machine (this machine has
  Claude Code installed separately from whatever session is reading this
  skill).
- Installs Aspire agent skill files into `.agents/skills/` (repo-local)
  and `~/.agents/skills/` (user-global): `aspire`, `aspire-init`,
  `aspire-monitoring`, `aspire-orchestration`, `aspire-deployment`. These
  are the actual, current documentation for CLI-driven Aspire agent
  workflows -- read them directly rather than trusting a summary.
- Adds a `PostToolUse` hook to Claude Code's settings
  (`~/.claude/settings.json` on Windows) that runs
  `~/.aspire/hooks/track-telemetry.ps1` after every tool call, to record
  Aspire skill/CLI/reference-file usage. Opt out with
  `ASPIRE_CLI_TELEMETRY_OPTOUT=true`.
- Does **NOT** register an MCP server. `claude mcp list` after running
  `aspire agent init` reported "No MCP servers configured." The
  `aspire agent mcp` subcommand exists (confirmed via `aspire agent
  --help`) but `aspire agent init` does not wire it up for Claude Code --
  the CLI-driven skill files are the integration, not a registered MCP
  tool set.

## One-time setup, if not already done

```
dotnet tool install --global Aspire.Cli --prerelease   # if `aspire` is not on PATH
aspire agent init --non-interactive                      # from the repo root
```

If `dotnet tool install` reports the tool already installed but `aspire`
still isn't found on PATH, the global-tool manifest can be stale --
`dotnet tool uninstall --global Aspire.Cli` then reinstall fixed exactly
this on the machine this was verified on.

## Verifying the connection

No `doctor` MCP tool call needed -- just run an actual command:

```
aspire ps --format Json
```

An empty or error result with no AppHost running is expected when nothing
is started; start one first (see the lifecycle commands below) before
expecting resource data back.

## Lifecycle commands -- use these, not `dotnet run` / manual process kill

The `aspire-orchestration` skill installed by `aspire agent init`
documents this as a **mandatory compliance** rule, citing
[microsoft/aspire#15801](https://github.com/microsoft/aspire/issues/15801):
`dotnet run --project <AppHost>` plus manually hunting down and
`Stop-Process`-ing orphaned child processes is exactly what causes the
`MSB3027`/`MSB3021`/`MSB3491` file-lock errors this repo hit repeatedly
earlier in this session. The correct pattern:

| Task | Command |
|------|---------|
| Start (background, for agents) | `aspire start --non-interactive` |
| Wait for a resource to be healthy | `aspire wait <resource> --non-interactive` |
| Inspect state | `aspire ps --format Json` (AppHost-level) / `aspire describe --format Json` (resource-level) |
| Stop cleanly | `aspire stop --non-interactive` |

Verified directly: `aspire start` detected and cleanly stopped a stray
prior instance before starting fresh (no manual PID hunting needed),
`aspire wait projectname-orchestrationapi` returned healthy, `aspire
describe --format Json` returned live state for all five resources
(`model-orchestrator`, `ollama-server`, `projectname-orchestrationapi`,
`projectname-runtime`, `repoRoot`) as `Running`/`Healthy`, and
`aspire stop` shut everything down with no orphaned processes left behind.
