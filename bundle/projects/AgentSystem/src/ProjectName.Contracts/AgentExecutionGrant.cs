namespace ProjectName.Contracts;

public sealed record AgentExecutionGrant(
    IReadOnlyList<string> DelegatedGrant,
    IReadOnlyList<string> HostMaximum,
    IReadOnlyList<string> EnvironmentMaximum,
    IReadOnlyList<string> Denies,
    string Source,
    string? GrantId = null,
    string? Subject = null,
    string? RegistrationRef = null,
    string? CapabilityId = null,
    string? CandidateDigest = null,
    IReadOnlyList<string>? ParentAuthority = null,
    IReadOnlyList<string>? EffectiveGrant = null,
    string? Issuer = null,
    string? IssuerProof = null,
    string? CheckerVerdictRef = null,
    string? ProvenanceRef = null,
    string? KeyId = null,
    int? KeyGeneration = null);

public sealed record AgentActivationReceipt(
    string Verdict,
    string MakerIdentity,
    string CheckerIdentity,
    string CandidateDigest,
    IReadOnlyDictionary<string, string> Files,
    AgentExecutionGrant Grant);
