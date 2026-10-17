namespace ProjectName.Marketplace;

using ProjectName.Contracts;

public sealed class MarketplaceResourceValidator : IResourceValidator
{
    public const string CreateSkillId = "create-skill";

    public ResourceValidationResult Validate(
        IEnumerable<string> requestedIds,
        AgentExecutionGrant? grant,
        IReadOnlySet<string> marketplaceCatalogIds)
    {
        var validated = new List<string>();
        var gaps = new List<GapRecord>();
        var granted = grant is null
            ? new HashSet<string>(StringComparer.Ordinal)
            : grant.DelegatedGrant
                .Intersect(grant.HostMaximum, StringComparer.Ordinal)
                .Intersect(grant.EnvironmentMaximum, StringComparer.Ordinal)
                .Except(grant.Denies, StringComparer.Ordinal)
                .ToHashSet(StringComparer.Ordinal);

        foreach (var id in requestedIds.Distinct(StringComparer.Ordinal))
        {
            if (!marketplaceCatalogIds.Contains(id))
            {
                gaps.Add(new GapRecord(
                    id,
                    "Not present in marketplace catalog",
                    "Author via create-skill, then REGISTER and grant before use",
                    CreateSkillId));
                continue;
            }
            // Catalog presence is discovery only. Use requires grant when grant is supplied.
            if (grant is not null && !granted.Contains(id) && id != CreateSkillId)
            {
                gaps.Add(new GapRecord(
                    id,
                    "In catalog but not granted",
                    "Request grant; do not invoke",
                    CreateSkillId));
                continue;
            }
            validated.Add(id);
        }

        return new ResourceValidationResult(gaps.Count == 0, validated, gaps);
    }
}
