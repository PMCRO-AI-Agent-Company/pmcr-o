namespace ProjectName.Contracts;

public enum TriggerSourceKind
{
    Human,
    Schedule,
    ReflectorNextSeed,
    ExecutiveDelegation,
    McpEvent,
    QueueMessage,
    Webhook,
    Agent
}

public sealed record TriggerEvent(
    string EventId,
    TriggerSourceKind SourceKind,
    string TrailId,
    string Intent,
    string SourceAgent,
    string TargetAgent,
    string SeedRef,
    string CausationId,
    string CorrelationId,
    IReadOnlyList<string> EvidenceRefs,
    string? ParentTrailId = null)
{
    public bool GrantsAuthority => false;
    public bool CanMutate => false;
}

public static class LifecycleOperations
{
    public const string ProvisionKeyGeneration = "provision-key-generation";
    public const string RegisterCapability = "register-capability";
    public const string IssueMinimumGrant = "issue-minimum-grant";
    public const string ActivateCapability = "activate-capability";
    public const string Revoke = "revoke";
    public const string ExecuteCapability = "execute-capability";
    public const string InspectState = "inspect-state";

    public static readonly IReadOnlySet<string> Mutating = new HashSet<string>(StringComparer.Ordinal)
    {
        ProvisionKeyGeneration, RegisterCapability, IssueMinimumGrant,
        ActivateCapability, Revoke, ExecuteCapability
    };
}

public sealed record LifecycleActionRequest(
    string RequestId,
    string Operation,
    string TrailId,
    string SeedRef,
    string CausationId,
    string CorrelationId,
    string AuthorizationRef,
    string CheckerVerdictRef,
    string ProvenanceRef,
    string AdmissionRef,
    string GrantVerificationRef,
    string CapabilityId,
    string SourceEventId,
    string CheckerVerdict,
    bool GrantVerified)
{
    public bool RequestsMutation => LifecycleOperations.Mutating.Contains(Operation);
    public bool GrantsAuthority => false;
}

public sealed record ActionReceipt(
    string ReceiptId,
    string RequestId,
    string TrailId,
    string Operation,
    string Status,
    string CausationId,
    string CorrelationId,
    string AuthorizationRef,
    string CheckerVerdictRef,
    string ProvenanceRef,
    IReadOnlyList<string> EvidenceRefs,
    DateTimeOffset CreatedAt)
{
    public bool GrantsAuthority => false;
}
