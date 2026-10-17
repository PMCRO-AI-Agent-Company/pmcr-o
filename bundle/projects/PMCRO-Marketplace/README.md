# PMCRO Marketplace

Cycle 2 introduces the smallest executable marketplace slice: read-only discovery of inherited global marketplace, plugin, and skill metadata.

## Run

```powershell
dotnet run --project src/PMCRO.Marketplace.Discovery/PMCRO.Marketplace.Discovery.csproj -- --global-root D:\AgentSystem\global --pretty
```

Optional filters: `--marketplace`, `--plugin`, and `--skill`.

The executable reuses `ProjectName.Marketplace.MarketplaceCatalogLoader` through a project reference. It reads marketplace metadata only. Registration, grant, activation, installation, publication, invocation, execution, revocation, and unregister operations are outside this slice and mutation-shaped options are refused.

Lifecycle law: `DISCOVERY != REGISTRATION != GRANT != ACTIVATION != EXECUTION`.
