namespace ProjectName.OrchestrationApi;

using ProjectName.Contracts;

/// <summary>
/// HTTP/JSON shape for @role /pmcro:phase.
/// Maps 1:1 to ProjectName.Contracts.PmcroRequest.
/// </summary>
public sealed class PmcroRequestEnvelope
{
    public string Command { get; init; } = "";
    public string Intent { get; init; } = "";
    public string AgentRef { get; init; } = "";
    public string? Route { get; init; }
    public string? Notes { get; init; }
    public string[]? ContextRefs { get; init; }
    public string? TrailRef { get; init; }
    public string? SeedRef { get; init; }
    public string? CycleRef { get; init; }
    public string? ParentToolUseId { get; init; }
    public string? AgentId { get; init; }
    public string? Framework { get; init; }
    public string[]? CapabilityRefs { get; init; }

    public PmcroRequest ToContract() => new(
        Command, Intent, AgentRef, Route, Notes, ContextRefs,
        TrailRef, SeedRef, CycleRef, ParentToolUseId, AgentId, Framework, CapabilityRefs);
}
