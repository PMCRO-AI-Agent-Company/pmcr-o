// MERGE into ProjectName.OrchestrationApi/Program.cs (do not replace existing /api/cycles):
//
// using ProjectName.Runtime;
//
// builder.Services.AddProjectNamePmcroRuntime(
//     autoRun: new AutoRunOptions { ContinuousWithinPhase = true },
//     mcpCatalogPath: builder.Configuration["Pmcro:McpCatalogPath"]);
//
// ... after app is built ...
// app.MapPmcroEndpoints();
//
// Existing:
//   POST /api/cycles          — full agent cycle via gRPC (intent-only body)
// New (template):
//   POST /api/pmcro/request   — phase-aware PmcroRequest envelope
//   GET  /api/pmcro/commands  — discovery of routes/roles/phases
