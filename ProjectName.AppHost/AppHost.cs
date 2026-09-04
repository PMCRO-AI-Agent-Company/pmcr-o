using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// The repository is the runtime's authoritative workspace. Keep the value
// parameterized so the AppHost never embeds a machine-specific drive/path.
var repoRoot = builder.AddParameter("repoRoot", () => Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..")));

// Persistent local Ollama model service.
var ollama = builder
    .AddOllama("ollama-server")
    .WithGPUSupport(OllamaGpuVendor.Nvidia)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("ollama-data")
    .WithEnvironment("OLLAMA_CONTEXT_LENGTH", "16384")
    .WithEnvironment("OLLAMA_FLASH_ATTENTION", "0");

var modelOrchestrator = ollama.AddModel("model-orchestrator", "qwen3:8b");

// Runtime is the model/agent execution boundary and exposes gRPC.
var runtime = builder.AddProject<Projects.ProjectName_Runtime>("projectname-runtime")
    .WithReference(ollama)
    .WithReference(modelOrchestrator)
    .WaitFor(modelOrchestrator);

// Thin HTTP/gRPC facade. HTTP chat calls cross the runtime boundary over gRPC.
builder.AddProject<Projects.ProjectName_OrchestrationApi>("projectname-orchestrationapi")
    .WithReference(runtime)
    .WithReference(modelOrchestrator)
    .WaitFor(runtime);

builder.Build().Run();
