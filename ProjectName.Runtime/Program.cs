using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Http.Resilience;
using ProjectName.Runtime.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => options.ConfigureEndpointDefaults(endpoint => endpoint.Protocols = HttpProtocols.Http1AndHttp2));
builder.AddServiceDefaults();
builder.AddOllamaApiClient("model-orchestrator")
    .AddChatClient();
builder.Services.Configure<HttpStandardResilienceOptions>("model-orchestrator", options =>
{
    options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
    options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(5);
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(10);
});
builder.Services.AddGrpc();

var app = builder.Build();
app.MapDefaultEndpoints();
app.MapGrpcService<RuntimeChatService>();

const string ollamaModel = "qwen3:8b";
var chatClient = app.Services.GetRequiredService<IChatClient>();
AIAgent agent = new ChatClientAgent(
    chatClient,
    instructions: "You are the ProjectName development agent. Be concise, factual, and execution-oriented.",
    name: "projectname-agent");

app.MapGet("/", () => Results.Ok(new
{
    service = "ProjectName.Runtime",
    transport = "gRPC",
    provider = "ollama",
    model = ollamaModel,
    status = "ready"
}));

app.MapGet("/chat", async (string prompt, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(prompt))
        return Results.BadRequest(new { error = "prompt is required" });

    var response = await agent.RunAsync($"/no_think\n{prompt}", cancellationToken: cancellationToken);
    return Results.Ok(new { model = ollamaModel, response = response.ToString() });
});

app.Run();
