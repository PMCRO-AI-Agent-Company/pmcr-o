using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Protocol;
using ProjectName.Harness;
using ProjectName.Tooling;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddProjectNameAgentSystem();
builder.Services.AddSingleton<IMcpOperationEligibility, DefaultMcpOperationEligibility>();
builder.Services.AddSingleton<DynamicPmcroMcpProjection>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithListToolsHandler((request, cancellationToken) =>
    {
        var services = request.Services ?? throw new InvalidOperationException("mcp-request-services-required");
        var projection = services.GetRequiredService<DynamicPmcroMcpProjection>();
        return ValueTask.FromResult(new ListToolsResult { Tools = projection.ListTools().ToList() });
    })
    .WithCallToolHandler((request, cancellationToken) =>
    {
        var services = request.Services ?? throw new InvalidOperationException("mcp-request-services-required");
        var projection = services.GetRequiredService<DynamicPmcroMcpProjection>();
        return ValueTask.FromResult(projection.Call(request.Params));
    });

await builder.Build().RunAsync();