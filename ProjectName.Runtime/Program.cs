using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Ollama;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

var ollamaEndpoint = Environment.GetEnvironmentVariable("OLLAMA_QWEN3_8B_URI")
    ?? throw new InvalidOperationException("OLLAMA_QWEN3_8B_URI is not set.");
var ollamaModel = Environment.GetEnvironmentVariable("OLLAMA_QWEN3_8B_MODEL")
    ?? "qwen3:8b";

IChatClient chatClient = new OllamaChatClient(new Uri(ollamaEndpoint), ollamaModel);
var agent = chatClient.AsAIAgent(
    instructions: "You are the ProjectName development agent. Be concise, factual, and execution-oriented.",
    name: "projectname-agent");

var app = builder.Build();
app.MapDefaultEndpoints();

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
    {
        return Results.BadRequest(new { error = "prompt is required" });
    }

    var response = await agent.RunAsync(prompt, cancellationToken: cancellationToken);
    return Results.Ok(new
    {
        model = ollamaModel,
        response = response.ToString()
    });
});

app.Run();

