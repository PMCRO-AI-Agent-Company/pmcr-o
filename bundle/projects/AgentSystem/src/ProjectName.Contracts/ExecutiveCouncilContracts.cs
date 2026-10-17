namespace ProjectName.Contracts;

public static class ExecutiveOfficeNames
{
    public const string ChiefExecutiveOfficer = "Chief Executive Officer";
    public const string ChiefFinancialOfficer = "Chief Financial Officer";
    public const string ChiefOperatingOfficer = "Chief Operating Officer";
    public const string ChiefTechnologyOfficer = "Chief Technology Officer";
    public const string ChiefInformationSecurityOfficer = "Chief Information Security Officer";
    public const string ChiefLegalOfficer = "Chief Legal Officer";
    public const string ChiefProductOfficer = "Chief Product Officer";
    public const string ChiefMarketingOfficer = "Chief Marketing Officer";
    public const string ChiefHumanResourcesOfficer = "Chief Human Resources Officer";
    public const string ChiefInformationOfficer = "Chief Information Officer";
    public const string ChiefRiskOfficer = "Chief Risk Officer";
    public const string ChiefComplianceOfficer = "Chief Compliance Officer";
    public const string ChiefDataOfficer = "Chief Data Officer";
    public const string ChiefArtificialIntelligenceOfficer = "Chief Artificial Intelligence Officer";
}

public sealed record ExecutiveOfficeDefinition(
    string OfficeId,
    string DisplayName,
    string Scope,
    string Instructions,
    IReadOnlyList<string> DomainTags,
    bool RequiresHumanApproval = false,
    int MaxOutputTokens = 2048)
{
    public string VisibleIdentity => $"I AM the {DisplayName} Agent.";
}

public sealed record ExecutiveCouncilRequest(
    string RequestId,
    string TrailId,
    string Question,
    IReadOnlyList<string> RequestedOfficeIds,
    IReadOnlyList<string> EvidenceRefs,
    int MaxDebateRounds = 1,
    bool RequireOwnerApproval = false);

public sealed record ExecutivePosition(
    string PositionId,
    string RequestId,
    string OfficeId,
    string Position,
    string Rationale,
    IReadOnlyList<string> Assumptions,
    IReadOnlyList<string> ConfirmedFacts,
    IReadOnlyList<string> UnverifiedClaims,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Risks,
    IReadOnlyList<string> Opportunities,
    IReadOnlyList<string> DissentRefs,
    IReadOnlyList<string> DelegationProposals,
    IReadOnlyList<string> NextSeedCandidates,
    double Confidence)
{
    public bool Authoritative => false;
    public bool GrantsAuthority => false;
}

public sealed record ExecutiveRecommendationBundle(
    string BundleId,
    string RequestId,
    IReadOnlyList<ExecutivePosition> Positions,
    IReadOnlyList<string> SharedFindings,
    IReadOnlyList<string> MaterialDisagreements,
    IReadOnlyList<string> EvidenceGaps,
    IReadOnlyList<string> Tradeoffs,
    string? RecommendedDecision,
    IReadOnlyList<string> AlternativeDecisions,
    IReadOnlyList<string> DelegationProposals,
    IReadOnlyList<string> Escalations,
    IReadOnlyList<string> NextSeedCandidates,
    bool RequiresOwnerApproval)
{
    public bool Authoritative => false;
    public bool GrantsAuthority => false;
}

public enum ExecutiveCouncilNextMode
{
    Complete,
    BoundedGroupChat,
    Handoff,
    HumanInTheLoop
}
