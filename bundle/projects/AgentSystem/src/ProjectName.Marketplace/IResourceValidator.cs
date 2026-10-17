namespace ProjectName.Marketplace;

using ProjectName.Contracts;

/// <summary>
/// Planner/Maker gate: only marketplace-listed + granted resources may be planned for use.
/// Unknown ids become gaps → create-skill path, never silent use.
/// </summary>
public interface IResourceValidator
{
    ResourceValidationResult Validate(
        IEnumerable<string> requestedIds,
        AgentExecutionGrant? grant,
        IReadOnlySet<string> marketplaceCatalogIds);
}
