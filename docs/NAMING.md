# Naming convention (template-driven)

PMCR-O is a **generic template**. All solution and package identifiers use the `ProjectName` placeholder.

| Surface | Pattern |
|---------|---------|
| Solution | `ProjectName.slnx` |
| Projects | `ProjectName.Contracts`, `ProjectName.Runtime`, `ProjectName.OrchestratorService`, … |
| Namespaces | `ProjectName.Contracts`, `ProjectName.Runtime`, … |
| Skills marketplace | `pmcro-skills` (marketplace id; not a C# package rename) |
| Human route | `@<role> /pmcro:<phase>` |
| Machine type | `PmcroRequest` (protocol name; lives under `ProjectName.Contracts`) |

Replace `ProjectName` when instantiating a concrete product. Do not hard-code a product brand into the template core.
