namespace ProjectName.Contracts;

public sealed record ExecutiveOrganizationManifest(
    string OrganizationId,
    string DisplayName,
    string Shape,
    IReadOnlyList<ExecutiveOfficeDefinition> Offices,
    IReadOnlyList<string> DefaultOfficeIds,
    IReadOnlyList<string> OwnerApprovalOfficeIds,
    string Version = "1")
{
    public bool GrantsAuthority => false;

    public IReadOnlyList<ExecutiveOfficeDefinition> ResolveOffices(IReadOnlyList<string>? requestedOfficeIds = null)
    {
        var requested = requestedOfficeIds is { Count: > 0 }
            ? requestedOfficeIds
            : DefaultOfficeIds;
        if (requested.Count == 0) return Offices;
        return Offices.Where(o => requested.Contains(o.OfficeId, StringComparer.Ordinal)).ToArray();
    }
}
