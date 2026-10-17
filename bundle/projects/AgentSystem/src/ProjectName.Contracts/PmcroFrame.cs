namespace ProjectName.Contracts;

/// <summary>
/// One ordered Frame inside an admitted Cycle.
/// Role (who) and cognitive phase (where in O→P→M→C→R) are independent axes.
/// All 25 combinations are legal cognitive identities; only the phase owner may commit authoritative phase state.
/// </summary>
public sealed record PmcroFrame(
    int Ordinal,
    string Role,
    string Phase,
    string FrameId)
{
    public string AuthoritativeOwner => PmcroFrameCatalog.AuthoritativeOwner(Phase);
    public bool IsAuthoritativeTurn => string.Equals(Role, AuthoritativeOwner, StringComparison.Ordinal);
}

public static class PmcroFrameCatalog
{
    public static readonly string[] Roles = PmcroConstants.Roles;
    public static readonly string[] Phases = PmcroConstants.Phases;

    private static readonly IReadOnlyDictionary<string, string> PhaseOwners =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ORCHESTRATE"] = "orchestrator",
            ["PLAN"] = "planner",
            ["MAKE"] = "maker",
            ["CHECK"] = "checker",
            ["REFLECT"] = "reflector"
        };

    /// <summary>Canonical cognitive order: for each role in Roles, all Phases in order (25 turns).</summary>
    public static IReadOnlyList<PmcroFrame> AllOrdered { get; } = Build();

    private static IReadOnlyList<PmcroFrame> Build()
    {
        var list = new List<PmcroFrame>(25);
        var i = 0;
        foreach (var role in Roles)
        foreach (var phase in Phases)
        {
            list.Add(new PmcroFrame(i, role, phase, role + ":" + phase));
            i++;
        }
        return list;
    }

    public static PmcroFrame? ById(string frameId) =>
        AllOrdered.FirstOrDefault(f => f.FrameId == frameId);

    public static PmcroFrame ByOrdinal(int ordinal) =>
        AllOrdered[ordinal];

    public static string Route(string role, string phase) =>
        "@" + role + " /pmcro:" + phase.ToLowerInvariant();

    public static string AuthoritativeOwner(string phase) =>
        PhaseOwners.TryGetValue(phase, out var owner)
            ? owner
            : throw new ArgumentOutOfRangeException(nameof(phase), phase, "Unknown cognitive phase");

    public static bool IsAuthoritative(string role, string phase) =>
        string.Equals(role, AuthoritativeOwner(phase), StringComparison.Ordinal);
}
