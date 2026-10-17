namespace ProjectName.Capabilities;

using ProjectName.Contracts;

/// <summary>Resolves MCP / host capabilities only when granted.
/// Catalog discovery does not imply grant.</summary>
public interface ICapabilityResolver
{
    bool IsGranted(string capabilityId, AgentExecutionGrant grant);
    IReadOnlyList<string> ListDiscoverable();
}
