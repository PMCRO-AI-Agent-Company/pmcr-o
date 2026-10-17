namespace ProjectName.Contracts;

public sealed record PmcroMessageEnvelope(
    string MessageId,
    string Kind,
    string TrailId,
    string SourceAgent,
    string TargetAgent,
    string PayloadJson,
    DateTimeOffset CreatedAt,
    string Status = "queued",
    string? ParentTrailId = null,
    string? SourceSessionId = null,
    string? TargetSessionId = null,
    string? SeedRef = null,
    string? CycleRef = null,
    string? FrameRef = null,
    IReadOnlyList<string>? EvidenceRefs = null,
    int Priority = 0,
    string? CausationId = null,
    string? CorrelationId = null,
    string? LeaseOwner = null,
    DateTimeOffset? LeaseExpiresAt = null)
{
    public bool GrantsAuthority => false;
    public IReadOnlyList<string> Evidence => EvidenceRefs ?? Array.Empty<string>();
}

public static class PmcroMessageKinds
{
    public const string SeedCandidate = "seed-candidate";
    public const string SeedAdmission = "seed-admission";
    public const string NextSeed = "next-seed";
    public const string ResumeTrail = "resume-trail";
    public const string ExecutiveRequest = "executive-request";
    public const string ExecutivePosition = "executive-position";
    public const string CouncilRecommendation = "council-recommendation";
    public const string EvidenceRequest = "evidence-request";
    public const string DelegationProposal = "delegation-proposal";
    public const string HumanDecisionRequest = "human-decision-request";
    public const string TriggerEvent = "trigger-event";
    public const string LifecycleActionRequest = "lifecycle-action-request";
    public const string ActionReceipt = "action-receipt";
}
