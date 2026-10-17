namespace ProjectName.Runtime.Maf;

using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

public interface IMafToolProvider
{
    Task<IReadOnlyList<AITool>> GetToolsAsync(CancellationToken cancellationToken = default);
}

public sealed record MafMcpToolProviderOptions(
    string Command,
    IReadOnlyList<string> Arguments,
    string Name = "ProjectName.AgentSystem.MCP",
    string? WorkingDirectory = null);

public sealed class StdioMcpToolProvider : IMafToolProvider, IAsyncDisposable
{
    private readonly McpClient _client;

    private StdioMcpToolProvider(McpClient client) => _client = client;

    public static async Task<StdioMcpToolProvider> CreateAsync(
        MafMcpToolProviderOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Command))
            throw new ArgumentException("mcp-command-required", nameof(options));

        var safeEnvironment = StdioClientTransportOptions.GetDefaultEnvironmentVariables();
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = options.Name,
            Command = options.Command,
            Arguments = options.Arguments.ToList(),
            WorkingDirectory = options.WorkingDirectory,
            InheritEnvironmentVariables = false,
            EnvironmentVariables = safeEnvironment,
        });

        var client = await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);
        return new StdioMcpToolProvider(client);
    }

    public async Task<IReadOnlyList<AITool>> GetToolsAsync(CancellationToken cancellationToken = default)
    {
        var tools = await _client.ListToolsAsync(cancellationToken: cancellationToken);
        return tools.Cast<AITool>().ToArray();
    }

    public ValueTask DisposeAsync() => _client.DisposeAsync();
}

public sealed class EmptyMafToolProvider : IMafToolProvider
{
    public Task<IReadOnlyList<AITool>> GetToolsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<AITool>>([]);
}