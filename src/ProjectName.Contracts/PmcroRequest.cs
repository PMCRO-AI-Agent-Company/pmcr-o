namespace ProjectName.Contracts;

/// <summary>
/// Machine envelope. Role and Phase are separate fields (25-frame model).
/// Human route remains @&lt;role&gt; /pmcro:&lt;phase&gt;.
/// Legacy <c>Command</c> mirrors phase lowercased for compatibility.
/// </summary>
public sealed record PmcroRequest(
    string Command,
    string Intent,
    string AgentRef,
    string? Route = null,
    string? Notes = null,
    IReadOnlyList<string>? ContextRefs = null,
    string? TrailRef = null,
    string? SeedRef = null,
    string? CycleRef = null,
    string? ParentToolUseId = null,
    string? AgentId = null,
    string? Framework = null,
    IReadOnlyList<string>? CapabilityRefs = null,
    string? Role = null,
    string? Phase = null,
    int? FrameOrdinal = null,
    string? FrameId = null)
{
    /// <summary>Resolved role: explicit Role, else parsed from AgentRef / route / command owner hint.</summary>
    public string ResolvedRole =>
        !string.IsNullOrEmpty(Role) ? Role
        : AgentRef.StartsWith("pmcro-", StringComparison.Ordinal) ? AgentRef["pmcro-".Length..]
        : AgentRef.StartsWith("pmcro.", StringComparison.Ordinal) ? AgentRef["pmcro.".Length..]
        : GuessRoleFromCommand(Command);

    public string ResolvedPhase =>
        !string.IsNullOrEmpty(Phase) ? Phase.ToUpperInvariant()
        : Command.ToUpperInvariant() is "INIT" or "SEED" ? ""
        : Command.ToUpperInvariant();

    public string ResolvedFrameId =>
        FrameId ?? (ResolvedRole + ":" + (string.IsNullOrEmpty(ResolvedPhase) ? "BOOTSTRAP" : ResolvedPhase));

    private static string GuessRoleFromCommand(string command) => command switch
    {
        "plan" => "planner",
        "make" => "maker",
        "check" => "checker",
        "reflect" => "reflector",
        _ => "orchestrator"
    };
}
