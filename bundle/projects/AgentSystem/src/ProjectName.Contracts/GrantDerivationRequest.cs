namespace ProjectName.Contracts;

public sealed record GrantDerivationRequest(
    string Subject,
    string RegistrationRef,
    string CapabilityId,
    string CandidateDigest,
    IReadOnlyList<string> Requested,
    IReadOnlyList<string> ParentAuthority,
    IReadOnlyList<string> HostMaximum,
    IReadOnlyList<string> EnvironmentMaximum,
    IReadOnlyList<string> Denies,
    string CheckerVerdictRef,
    string ProvenanceRef);
