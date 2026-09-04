using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => options.ConfigureEndpointDefaults(endpoint => endpoint.Protocols = HttpProtocols.Http1AndHttp2));
builder.AddServiceDefaults();
builder.AddOllamaApiClient("qwen3:8b");

var app = builder.Build();
app.MapDefaultEndpoints();

var ollamaModel = "qwen3:8b";
var chatClient = app.Services.GetRequiredService<IChatClient>();
AIAgent agent = new ChatClientAgent(
    chatClient,
    instructions: "You are the ProjectName development agent. Be concise, factual, and execution-oriented.",
    name: "projectname-agent");

app.MapGet("/", () => Results.Ok(new
{
    service = "ProjectName.Runtime",
    provider = "ollama",
    model = ollamaModel,
    status = "ready"
}));

app.MapGet("/chat", async (string prompt, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(prompt))
        return Results.BadRequest(new { error = "prompt is required" });

    var response = await agent.RunAsync(prompt, cancellationToken: cancellationToken);
    return Results.Ok(new { model = ollamaModel, response = response.ToString() });
});

app.Run();
