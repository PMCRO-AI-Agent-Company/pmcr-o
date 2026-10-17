using System.Text.Json;
using ProjectName.Marketplace;

const string ApiVersion = "pmcro.marketplace.discovery/v1";
const string Kind = "MarketplaceDiscoveryResult";

var parsed = ParseArgs(args);
if (parsed.Help)
{
    Console.WriteLine("PMCRO Marketplace Discovery\n\nUsage:\n  dotnet run --project PMCRO.Marketplace.Discovery.csproj -- --global-root <path> [--marketplace <id>] [--plugin <id>] [--skill <id>] [--pretty]\n\nRead-only discovery only. Registration, grant, activation, installation, publication, invocation, and execution options are refused.");
    return 0;
}

var globalRoot = Path.GetFullPath(parsed.GlobalRoot ?? Environment.GetEnvironmentVariable("AGENTSYSTEM_GLOBAL_ROOT") ?? string.Empty);
if (string.IsNullOrWhiteSpace(parsed.GlobalRoot) && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AGENTSYSTEM_GLOBAL_ROOT")))
    return Fail("--global-root or AGENTSYSTEM_GLOBAL_ROOT is required");
if (!Directory.Exists(globalRoot)) return Fail("global root does not exist");

var marketplacesConfig = Path.Combine(globalRoot, ".agents", "marketplaces.json");
if (!File.Exists(marketplacesConfig)) return Fail("global .agents/marketplaces.json is missing");

using var configDoc = JsonDocument.Parse(File.ReadAllText(marketplacesConfig));
if (!configDoc.RootElement.TryGetProperty("sources", out var sources) || sources.ValueKind != JsonValueKind.Array)
    return Fail("marketplace sources array is missing");

var records = new List<SkillDiscoveryRecord>();
var skillDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
var marketplaceCount = 0;
var pluginCount = 0;

foreach (var source in sources.EnumerateArray())
{
    if (source.TryGetProperty("enabled", out var enabled) && enabled.ValueKind == JsonValueKind.False) continue;
    var sourceId = GetRequiredString(source, "id");
    var sourceType = GetRequiredString(source, "type");
    if (!string.Equals(sourceType, "directory", StringComparison.Ordinal)) continue;
    var relativePath = GetRequiredString(source, "path");
    var marketplaceRoot = ContainedPath(globalRoot, relativePath);
    if (!Directory.Exists(marketplaceRoot)) continue;

    var marketplaceJson = Path.Combine(marketplaceRoot, "marketplace.json");
    if (!File.Exists(marketplaceJson)) continue;
    using var marketplaceDoc = JsonDocument.Parse(File.ReadAllText(marketplaceJson));
    var marketplaceName = GetMetadataString(marketplaceDoc.RootElement, "name") ?? sourceId;
    var marketplaceVersion = GetMetadataString(marketplaceDoc.RootElement, "version") ?? "unknown";
    if (parsed.Marketplace is not null && !string.Equals(parsed.Marketplace, marketplaceName, StringComparison.Ordinal)) continue;
    marketplaceCount++;

    if (!marketplaceDoc.RootElement.TryGetProperty("spec", out var marketplaceSpec) ||
        !marketplaceSpec.TryGetProperty("plugins", out var plugins) || plugins.ValueKind != JsonValueKind.Array) continue;

    foreach (var pluginValue in plugins.EnumerateArray())
    {
        var pluginId = pluginValue.GetString();
        if (string.IsNullOrWhiteSpace(pluginId)) continue;
        if (parsed.Plugin is not null && !string.Equals(parsed.Plugin, pluginId, StringComparison.Ordinal)) continue;

        var pluginRoot = ContainedPath(marketplaceRoot, Path.Combine("plugins", pluginId));
        var pluginJson = Path.Combine(pluginRoot, "plugin.json");
        if (!File.Exists(pluginJson)) continue;
        using var pluginDoc = JsonDocument.Parse(File.ReadAllText(pluginJson));
        var pluginName = GetMetadataString(pluginDoc.RootElement, "name") ?? pluginId;
        var pluginVersion = GetMetadataString(pluginDoc.RootElement, "version") ?? "unknown";
        var declaredDiscover = ReadDeclaredDiscover(pluginDoc.RootElement);
        pluginCount++;

        if (!pluginDoc.RootElement.TryGetProperty("spec", out var pluginSpec) ||
            !pluginSpec.TryGetProperty("skills", out var skills) || skills.ValueKind != JsonValueKind.Array) continue;

        var skillsRoot = ContainedPath(pluginRoot, "skills");
        if (Directory.Exists(skillsRoot)) skillDirectories.Add(skillsRoot);

        foreach (var skillValue in skills.EnumerateArray())
        {
            var skillId = skillValue.GetString();
            if (string.IsNullOrWhiteSpace(skillId)) continue;
            if (parsed.Skill is not null && !string.Equals(parsed.Skill, skillId, StringComparison.Ordinal)) continue;

            var skillRoot = ContainedPath(skillsRoot, skillId);
            if (!Directory.Exists(skillRoot)) continue;
            var skillMd = Path.Combine(skillRoot, "SKILL.md");
            var frontmatter = ReadFrontmatter(skillMd);
            var agents = EnumerateRelative(skillRoot, "agents", "*.md");
            var scripts = EnumerateRelative(skillRoot, "scripts", "*");

            records.Add(new SkillDiscoveryRecord(
                MarketplaceId: marketplaceName,
                MarketplaceVersion: marketplaceVersion,
                PluginId: pluginName,
                PluginVersion: pluginVersion,
                SkillId: skillId,
                SkillName: frontmatter.Name ?? skillId,
                Description: frontmatter.Description,
                SkillPath: Relative(globalRoot, skillRoot),
                AgentFiles: agents,
                ScriptFiles: scripts,
                DeclaredDiscoveryAuthority: declaredDiscover,
                Lifecycle: LifecycleObservation.DiscoveryOnly));
        }
    }
}

var loaderIds = MarketplaceCatalogLoader.LoadIds(null, skillDirectories.OrderBy(x => x, StringComparer.Ordinal).ToArray());
var ordered = records
    .OrderBy(r => r.MarketplaceId, StringComparer.Ordinal)
    .ThenBy(r => r.PluginId, StringComparer.Ordinal)
    .ThenBy(r => r.SkillId, StringComparer.Ordinal)
    .ToArray();
var parity = ordered.All(r => loaderIds.Contains(r.SkillId));

var result = new DiscoveryResult(
    ApiVersion,
    Kind,
    ProjectRef: "PMCRO-Marketplace",
    SourceRoot: globalRoot.Replace('\\', '/'),
    ReadOnly: true,
    LifecycleSeparation: LifecycleObservation.DiscoveryOnly,
    Reuse: new ReuseEvidence(
        "ProjectName.Marketplace.MarketplaceCatalogLoader",
        "ProjectReference",
        parity,
        loaderIds.Count),
    Summary: new DiscoverySummary(marketplaceCount, pluginCount, ordered.Length),
    Records: ordered);

var jsonOptions = new JsonSerializerOptions { WriteIndented = parsed.Pretty };
Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));
return parity ? 0 : Fail("MarketplaceCatalogLoader parity check failed");

static int Fail(string message)
{
    Console.Error.WriteLine(message);
    return 2;
}

static ParsedArgs ParseArgs(string[] args)
{
    var result = new ParsedArgs();
    var mutationOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "--register", "--grant", "--activate", "--install", "--publish", "--invoke", "--execute", "--unregister", "--revoke"
    };
    for (var i = 0; i < args.Length; i++)
    {
        var arg = args[i];
        if (mutationOptions.Contains(arg)) throw new ArgumentException($"mutation option refused: {arg}");
        switch (arg)
        {
            case "--help": case "-h": result.Help = true; break;
            case "--pretty": result.Pretty = true; break;
            case "--global-root": result.GlobalRoot = Next(args, ref i, arg); break;
            case "--marketplace": result.Marketplace = Next(args, ref i, arg); break;
            case "--plugin": result.Plugin = Next(args, ref i, arg); break;
            case "--skill": result.Skill = Next(args, ref i, arg); break;
            default: throw new ArgumentException($"unknown option refused: {arg}");
        }
    }
    return result;
}

static string Next(string[] args, ref int index, string option)
{
    if (++index >= args.Length) throw new ArgumentException($"missing value for {option}");
    return args[index];
}

static string GetRequiredString(JsonElement element, string property)
{
    if (!element.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()))
        throw new InvalidDataException($"required string missing: {property}");
    return value.GetString()!;
}

static string? GetMetadataString(JsonElement root, string property)
{
    if (!root.TryGetProperty("metadata", out var metadata) || !metadata.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.String)
        return null;
    return value.GetString();
}

static bool ReadDeclaredDiscover(JsonElement root)
{
    if (root.TryGetProperty("authority", out var authority) && authority.TryGetProperty("discover", out var discover) && discover.ValueKind is JsonValueKind.True or JsonValueKind.False)
        return discover.GetBoolean();
    if (root.TryGetProperty("spec", out var spec) && spec.TryGetProperty("authority", out var nested) && nested.TryGetProperty("discover", out var nestedDiscover) && nestedDiscover.ValueKind is JsonValueKind.True or JsonValueKind.False)
        return nestedDiscover.GetBoolean();
    return false;
}

static string ContainedPath(string root, string relative)
{
    var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
    var full = Path.GetFullPath(Path.Combine(root, relative));
    if (!full.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("marketplace path escapes global root");
    return full;
}

static string Relative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');

static string[] EnumerateRelative(string skillRoot, string childDirectory, string pattern)
{
    var dir = Path.Combine(skillRoot, childDirectory);
    if (!Directory.Exists(dir)) return [];
    return Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly)
        .Select(path => Relative(skillRoot, path))
        .OrderBy(path => path, StringComparer.Ordinal)
        .ToArray();
}

static SkillFrontmatter ReadFrontmatter(string path)
{
    if (!File.Exists(path)) return new SkillFrontmatter(null, null);
    var lines = File.ReadAllLines(path);
    if (lines.Length == 0 || lines[0].Trim() != "---") return new SkillFrontmatter(null, null);
    string? name = null; string? description = null;
    for (var i = 1; i < lines.Length; i++)
    {
        var line = lines[i].Trim();
        if (line == "---") break;
        var split = line.IndexOf(':');
        if (split <= 0) continue;
        var key = line[..split].Trim();
        var value = line[(split + 1)..].Trim().Trim('"');
        if (key == "name") name = value;
        if (key == "description") description = value;
    }
    return new SkillFrontmatter(name, description);
}

sealed class ParsedArgs
{
    public bool Help { get; set; }
    public bool Pretty { get; set; }
    public string? GlobalRoot { get; set; }
    public string? Marketplace { get; set; }
    public string? Plugin { get; set; }
    public string? Skill { get; set; }
}

sealed record SkillFrontmatter(string? Name, string? Description);
sealed record LifecycleObservation(bool Discovery, string Registration, string Grant, string Activation, string Execution)
{
    public static LifecycleObservation DiscoveryOnly { get; } = new(true, "not-evaluated", "not-evaluated", "not-evaluated", "not-evaluated");
}
sealed record ReuseEvidence(string Component, string Binding, bool CatalogParity, int ExistingCatalogIdCount);
sealed record DiscoverySummary(int Marketplaces, int Plugins, int Skills);
sealed record SkillDiscoveryRecord(
    string MarketplaceId,
    string MarketplaceVersion,
    string PluginId,
    string PluginVersion,
    string SkillId,
    string SkillName,
    string? Description,
    string SkillPath,
    string[] AgentFiles,
    string[] ScriptFiles,
    bool DeclaredDiscoveryAuthority,
    LifecycleObservation Lifecycle);
sealed record DiscoveryResult(
    string ApiVersion,
    string Kind,
    string ProjectRef,
    string SourceRoot,
    bool ReadOnly,
    LifecycleObservation LifecycleSeparation,
    ReuseEvidence Reuse,
    DiscoverySummary Summary,
    SkillDiscoveryRecord[] Records);
