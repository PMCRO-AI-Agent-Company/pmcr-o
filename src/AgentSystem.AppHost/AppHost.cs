using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
var ollama = builder.AddOllama("agentsystem-ollama")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("agentsystem-ollama-data")
    .WithEnvironment("OLLAMA_CONTEXT_LENGTH", "16384")
    .WithEnvironment("OLLAMA_FLASH_ATTENTION", "0");
if (builder.Configuration.GetValue("AgentSystem:UseGpu", true))
    ollama.WithGPUSupport(OllamaGpuVendor.Nvidia);
var model = ollama.AddModel("agentsystem-model", "qwen3:8b");
var postgres = builder.AddPostgres("agentsystem-postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("agentsystem-postgres-data");
var database = postgres.AddDatabase("agentsystem");
var api = builder.AddProject<Projects.ProjectName_OrchestrationApi>("agentsystem-api")
    .WithReference(ollama).WithReference(model).WithReference(database)
    .WaitFor(model).WaitFor(database)
    .WithEnvironment("AgentSystem__ProductionHost", "true")
    .WithEnvironment("AgentSystem__Model", "qwen3:8b");
// Trust remains operator deployment input; topology/model availability grants nothing.
var trust = Environment.GetEnvironmentVariable("PMCRO_CONTROL_PLANE_TRUST_FILE");
if (!string.IsNullOrWhiteSpace(trust)) api.WithEnvironment("PMCRO_CONTROL_PLANE_TRUST_FILE", trust);
builder.Build().Run();
