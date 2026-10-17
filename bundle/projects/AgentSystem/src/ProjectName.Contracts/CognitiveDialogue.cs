namespace ProjectName.Contracts;

public static class PmcroRoleIdentity
{
    public static string FullName(string role) => role.ToLowerInvariant() switch
    {
        "orchestrator" => "Orchestrator",
        "planner" => "Planner",
        "maker" => "Maker",
        "checker" => "Checker",
        "reflector" => "Reflector",
        _ => role
    };
}

public static class DialogueDependencyRelations
{
    public const string Root = "ROOT";
    public const string Accepts = "ACCEPTS";
    public const string Challenges = "CHALLENGES";
    public const string Extends = "EXTENDS";
    public const string Requests = "REQUESTS";
    public const string Verifies = "VERIFIES";
}

public sealed record CognitiveDialogueTurn(
    string TurnId,
    string SpeakerRole,
    string CognitiveLens,
    IReadOnlyList<string> AddressedTo,
    string? Statement,
    string? Question,
    string? Challenge,
    string? Response,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> PriorBeliefRefs,
    string? RevisedBelief,
    IReadOnlyList<string> UnresolvedTensions,
    string? SelfReference,
    string? AuthoritativeFrameRef,
    string? SpeakerFullName = null,
    string DependencyRelation = DialogueDependencyRelations.Root,
    IReadOnlyList<string>? DependsOnTurnRefs = null,
    string? NextSpeakerRole = null,
    string? NextDependency = null,
    string? NextSeedRef = null,
    bool ChainClosed = false)
{
    public string AuthoritativeOwner => PmcroFrameCatalog.AuthoritativeOwner(CognitiveLens);
    public bool IsAuthoritativeTurn => string.Equals(SpeakerRole, AuthoritativeOwner, StringComparison.Ordinal);
    public bool GrantsAuthority => false;
    public string VisibleRoleName => SpeakerFullName ?? PmcroRoleIdentity.FullName(SpeakerRole);
    public string IdentityStatement => $"I AM the {VisibleRoleName} Agent.";
    public IReadOnlyList<string> DependsOn => DependsOnTurnRefs ?? Array.Empty<string>();
    public bool HasCausalContinuation => ChainClosed || !string.IsNullOrWhiteSpace(NextSpeakerRole) || !string.IsNullOrWhiteSpace(NextSeedRef);
}
