using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.AI;
using ProjectName.Runtime.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => options.ConfigureEndpointDefaults(endpoint => endpoint.Protocols = HttpProtocols.Http1AndHttp2));
builder.AddServiceDefaults();
builder.AddOllamaApiClient("model-orchestrator")
    .AddChatClient();
// The 5-minute resilience timeout this client needs is now set once, for
// every HttpClient in the process, in ProjectName.ServiceDefaults'
// AddServiceDefaults() -- a per-client Configure<HttpStandardResilienceOptions>
// override here previously did not work (named-options key mismatch) and
// has been removed rather than left as dead, misleading code.
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
