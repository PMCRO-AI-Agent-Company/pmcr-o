// MERGE into AppHost or service Program.cs composition
// using ProjectName.Runtime;
//
// builder.Services.AddProjectNamePmcroRuntime(
//     autoRun: new AutoRunOptions
//     {
//         ContinuousWithinPhase = true,
//         MaxStepsPerPhase = 32,
//         RequireIndependentCheck = true,
//         RequirePriorFrameworkActivation = true,
//         HaltOnFail = true
//     },
//     mcpCatalogPath: Path.Combine(builder.Environment.ContentRootPath,
//         "..", "..", "pmcro-skills", ".agents", "capabilities", "mcp-catalog.json"),
//     frameStartOrdinal: 0);
