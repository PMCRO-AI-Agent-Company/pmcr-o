namespace ProjectName.Contracts;

public sealed record OrchestratorProductDefinition(
    string ProductId,
    string DisplayName,
    string VisibleIdentity,
    string RoutingId,
    string Domain,
    string TrailNamespace,
    string InitRoute,
    IReadOnlyList<string> MarketplaceRefs,
    IReadOnlyList<string> PluginRefs,
    IReadOnlyList<string> AgentRefs,
    IReadOnlyList<string> AuthorityCeiling)
{
    public bool GrantsAuthority => false;
}

public sealed record TrailProductDescriptor(
    string TrailId,
    string TrailUri,
    string RootTrailId,
    string? ParentTrailId,
    string OrchestratorProductId,
    string OwnerAgent,
    string SeedRef,
    string? TrueIntentRef,
    string Status,
    IReadOnlyList<string> AuthorityCeiling,
    DateTimeOffset CreatedAt,
    bool Resumable = true,
    bool Forkable = true,
    bool Exportable = true,
    bool Replayable = true)
{
    public bool GrantsAuthority => false;
}

public sealed record ChildTrailLaunch(
    TrailProductDescriptor Trail,
    string InitRoute,
    string CausationId,
    string CorrelationId,
    string ParentTrueIntentRef)
{
    public bool GrantsAuthority => false;
}

public sealed record ChildTrailEvidenceReturn(
    string ParentTrailId,
    string ChildTrailId,
    string CorrelationId,
    IReadOnlyList<string> EvidenceRefs,
    string Status,
    DateTimeOffset CreatedAt)
{
    public bool GrantsAuthority => false;
}

public sealed record TrailProgressReceipt(
    string TrailId,
    string CorrelationId,
    string Stage,
    string Status,
    int PercentComplete,
    IReadOnlyList<string> EvidenceRefs,
    DateTimeOffset CreatedAt)
{
    public bool GrantsAuthority => false;
}

public static class TrailProductMessageKinds
{
    public const string OrchestratorProductSelected = "orchestrator-product-selected";
    public const string ChildTrailCreated = "child-trail-created";
    public const string ChildTrailEvidenceReturned = "child-trail-evidence-returned";
    public const string TrailProgress = "trail-progress";
}
