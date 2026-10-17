namespace ProjectName.Runtime.Maf;

using System.Text.Json;
using ProjectName.Contracts;

public static class ExecutiveOrganizationManifestLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static ExecutiveOrganizationManifest Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(path);
        var json = File.ReadAllText(fullPath);
        return Parse(json);
    }

    public static ExecutiveOrganizationManifest Parse(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var manifest = JsonSerializer.Deserialize<ExecutiveOrganizationManifest>(json, JsonOptions)
            ?? throw new InvalidOperationException("executive-organization-manifest-invalid");
        Validate(manifest);
        return manifest;
    }

    public static void Validate(ExecutiveOrganizationManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        if (string.IsNullOrWhiteSpace(manifest.OrganizationId))
            throw new InvalidOperationException("organization-id-required");
        if (string.IsNullOrWhiteSpace(manifest.DisplayName))
            throw new InvalidOperationException("organization-display-name-required");
        if (string.IsNullOrWhiteSpace(manifest.Shape))
            throw new InvalidOperationException("organization-shape-required");
        if (manifest.Offices.Count == 0)
            throw new InvalidOperationException("executive-offices-required");

        var duplicate = manifest.Offices
            .GroupBy(o => o.OfficeId, StringComparer.Ordinal)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
            throw new InvalidOperationException("duplicate-executive-office-id:" + duplicate.Key);

        var officeIds = manifest.Offices.Select(o => o.OfficeId).ToHashSet(StringComparer.Ordinal);
        var unknownDefault = manifest.DefaultOfficeIds.FirstOrDefault(id => !officeIds.Contains(id));
        if (unknownDefault is not null)
            throw new InvalidOperationException("unknown-default-executive-office:" + unknownDefault);

        var unknownApproval = manifest.OwnerApprovalOfficeIds.FirstOrDefault(id => !officeIds.Contains(id));
        if (unknownApproval is not null)
            throw new InvalidOperationException("unknown-owner-approval-office:" + unknownApproval);
    }
}
