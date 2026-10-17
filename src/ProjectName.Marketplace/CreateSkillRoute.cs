namespace ProjectName.Marketplace;

using ProjectName.Contracts;

/// <summary>
/// When gaps exist, emit a seed-shaped intent for create-skill authoring.
/// Does not REGISTER, grant, or execute the missing capability.
/// </summary>
public static class CreateSkillRoute
{
    public static NextSeedIntent FromGaps(
        IReadOnlyList<GapRecord> gaps,
        string cycleRef,
        string frameId)
    {
        var ids = string.Join(", ", gaps.Select(g => g.RequestedId));
        return new NextSeedIntent(
            Intent: "create-skill for missing or ungranted capabilities: " + ids,
            SourceCycleRef: cycleRef,
            SourceFrameId: frameId,
            SuggestedRole: "orchestrator",
            Notes: "Author skills via create-skill only. REGISTER and grant are separate lifecycle steps. Do not auto-execute.");
    }
}
