# PMCR-O deterministic runtime engine

These scripts implement only durable lifecycle mechanics. They do not perform
Planner/Maker/Checker/Reflector reasoning and they do not replace the shipped
role plugins.

## Execution boundary

1. `Enqueue-SeedIntent.ps1` captures an immutable `messy_seed` into the queue.
2. `Invoke-PmcroCycle.ps1` claims one pending seed, mints a Class-B trail,
   records the Orchestrator frame, and hands off to Planner.
3. The five lifecycle roles operate on their owned phase file through their
   existing plugin scripts.
4. `Seal-PmcroTrail.ps1` is the final seal gate. It is callable by the
   Reflector lifecycle adapter only and refuses to seal without Checker PASS
   plus non-empty criterion evidence.
5. `Test-PmcroTrail.ps1` is read-only structural/governance verification.

## Ownership

| Artifact | Owner |
|---|---|
| `queue/` | runtime intake + Orchestrator claim lifecycle |
| `trails/<guid>/orchestrate.jsonl` | Orchestrator |
| `trails/<guid>/plan.jsonl` | Planner |
| `trails/<guid>/make.jsonl` | Maker |
| `trails/<guid>/check.jsonl` | Checker |
| `trails/<guid>/reflect.jsonl` | Reflector |
| `trails/<guid>/disposition.json` seal bit | Reflector through `Seal-PmcroTrail.ps1` |

The engine deliberately stops at the role boundary. A deterministic script
must never fabricate a PlanFrame, execution result, verification result, or
Reflection merely to make a cycle appear complete.
