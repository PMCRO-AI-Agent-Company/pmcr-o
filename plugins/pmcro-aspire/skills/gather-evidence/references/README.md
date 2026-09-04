# References -- pmcro-aspire:gather-evidence

## Asset map

| Asset | Purpose |
|---|---|
| `assets/setup.gather-evidence.asset.md` | what `aspire agent init` actually installs (skill files + a telemetry hook, no MCP server), and the safe `aspire start/wait/stop` lifecycle commands |
| `assets/tools.gather-evidence.asset.md` | the CLI command catalog, grouped by evidence use |
| `assets/recipe.gather-evidence.asset.md` | how Maker/Checker fold command output into trail evidence |

## Project-local skill files (installed by `aspire agent init`)

`.agents/skills/aspire`, `.agents/skills/aspire-init`,
`.agents/skills/aspire-monitoring`, `.agents/skills/aspire-orchestration`,
`.agents/skills/aspire-deployment` -- these are the authoritative, current
Aspire CLI documentation for this repo. When this skill's summaries and
those files disagree, the project-local skill files win; update this
skill's assets to match rather than the other way around.

## Source URLs and history

- https://aspire.dev/get-started/aspire-mcp-server/ -- fetched
  2026-09-04, recorded in trail `ba0c2c65-075f-470b-abed-e5647053dc8b`
  (MakeStep 2). Described a 14-tool MCP server (`aspire agent mcp`) that
  version 0.1.0 of this skill documented as the setup path, without
  running it against this repo.
- Trail `3a9ec041-f2a4-4e06-864f-a8f723201122` -- built version 0.1.0 of
  this skill from the above research alone.
- Trail `d360b692-5014-4267-9018-9b94758e9170` -- actually ran
  `aspire agent init` against this repo (2026-09-04) and found the MCP
  server was never registered (`claude mcp list` -> "No MCP servers
  configured"); the real mechanism is the Aspire CLI's local backchannel,
  documented by the skill files `aspire agent init` installs. Rewrote
  every asset in this skill to match what was actually observed. Also
  verified live: `aspire start`, `aspire wait`, `aspire describe --format
  Json` (all five resources Running/Healthy), and `aspire stop` all
  behaved exactly as their respective skill files describe.
