using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder.AddOllama("ollama")
    .WithDataVolume()
    .WithGPUSupport(OllamaGpuVendor.Nvidia);

var qwen = ollama.AddModel("qwen3:8b");

builder.AddProject<Projects.ProjectName_Runtime>("projectname-runtime")
    .WithReference(qwen)
    .WaitFor(qwen);

builder.Build().Run();
