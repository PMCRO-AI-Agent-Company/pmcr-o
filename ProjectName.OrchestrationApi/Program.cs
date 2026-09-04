using Grpc.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ProjectName.OrchestrationApi.Services;
using ProjectName.Runtime;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => options.ConfigureEndpointDefaults(endpoint => endpoint.Protocols = HttpProtocols.Http1AndHttp2));
builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddGrpc();
builder.Services.AddGrpcClient<RuntimeChat.RuntimeChatClient>(options =>
{
    options.Address = new Uri("https://projectname-runtime");
});
builder.Services.AddScoped<RuntimeGatewayService>();

var app = builder.Build();
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "ProjectName Orchestration API";
        options.Theme = ScalarTheme.Mars;
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<GreeterService>();

app.MapGet("/", () => Results.Ok(new
{
    service = "ProjectName.OrchestrationApi",
    transport = "HTTP/1.1 + HTTP/2",
    runtime = "projectname-runtime",
    modelPath = "API -> Runtime gRPC -> MAF/IChatClient -> Ollama"
}));

app.MapGet("/api/chat", async (string prompt, RuntimeGatewayService runtime, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(prompt))
        return Results.BadRequest(new { error = "prompt is required" });

    try
    {
        var response = await runtime.ChatAsync(prompt, cancellationToken);
        return Results.Ok(new { model = response.Model, response = response.Response });
    }
    catch (RpcException ex)
    {
        return Results.Problem(detail: ex.Status.Detail, statusCode: ex.StatusCode switch
        {
            StatusCode.InvalidArgument => StatusCodes.Status400BadRequest,
            StatusCode.Unavailable => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status502BadGateway
        });
    }
}).WithName("Chat").WithSummary("Send a prompt through the runtime gRPC gateway to Ollama");

app.Run();
