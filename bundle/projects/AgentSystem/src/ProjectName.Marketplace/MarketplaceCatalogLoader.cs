namespace ProjectName.Marketplace;

using System.Text.Json;

public static class MarketplaceCatalogLoader
{
    /// <summary>Loads skill folder names + MCP capability ids. Discovery only.</summary>
    public static HashSet<string> LoadIds(string? mcpCatalogPath, params string[] skillDirectories)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var dir in skillDirectories)
        {
            if (!Directory.Exists(dir)) continue;
            foreach (var d in Directory.GetDirectories(dir))
                set.Add(Path.GetFileName(d));
        }
        if (mcpCatalogPath is not null && File.Exists(mcpCatalogPath))
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(mcpCatalogPath));
            if (doc.RootElement.TryGetProperty("capabilities", out var caps))
                foreach (var c in caps.EnumerateArray())
                    if (c.TryGetProperty("id", out var id))
                        set.Add(id.GetString()!);
        }
        set.Add(MarketplaceResourceValidator.CreateSkillId);
        return set;
    }
}
