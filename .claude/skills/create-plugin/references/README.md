# References — create-plugin

## Asset map

| Asset | Purpose |
|---|---|
| `assets/packaging-checklist.asset.md` | the full checklist: manifest shape, agent filenames, zip-building, verification, logging |
| `scripts/New-PluginZip.ps1` | deterministic zip-build implementation for one plugin (zero reasoning) — never hand-roll `Compress-Archive` or a one-off zip script again |
| `scripts/Update-PluginZips.ps1` | incremental batch wrapper: rebuilds every plugin under `plugins/` whose source changed since its zip was last built (or has no zip yet), via `New-PluginZip.ps1` |

## Source URLs

None. Every rule in `assets/packaging-checklist.asset.md` was discovered by
an actual failed Cowork upload or a failed `claude plugin validate` call in
this repo, not ported from external docs. When content is later ported
from elsewhere, its origin URL is recorded here, not inline in `SKILL.md`
or the asset above.
