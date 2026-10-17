namespace ProjectName.Capabilities;

using System.Text.Json;
using ProjectName.Contracts;

/// <summary>Loads declaration-only MCP catalog; IsGranted requires capability id in grant lists.</summary>
public sealed class McpCapabilityResolver : ICapabilityResolver
{
    private readonly List<string> _discoverable = new();

    public McpCapabilityResolver(string? catalogPath = null)
    {
        if (catalogPath is null || !File.Exists(catalogPath))
        {
            _discoverable.AddRange(["mcp.terminal", "mcp.playwright", "mcp.filesystem", "mcp.git"]);
            return;
        }
        using var doc = JsonDocument.Parse(File.ReadAllText(catalogPath));
        foreach (var c in doc.RootElement.GetProperty("capabilities").EnumerateArray())
            _discoverable.Add(c.GetProperty("id").GetString()!);
    }

    public IReadOnlyList<string> ListDiscoverable() => _discoverable;

    public bool IsGranted(string capabilityId, AgentExecutionGrant grant)
    {
        if (grant.Denies.Contains(capabilityId, StringComparer.Ordinal))
            return false;
        var pool = grant.DelegatedGrant
            .Intersect(grant.HostMaximum, StringComparer.Ordinal)
            .Intersect(grant.EnvironmentMaximum, StringComparer.Ordinal);
        return pool.Contains(capabilityId, StringComparer.Ordinal);
    }
}
