namespace ProjectName.Contracts;

public sealed record CapabilityRegistration(
    string RegistrationId,
    string CapabilityId,
    string CandidateDigest,
    string FileManifestDigest,
    string CheckerVerdictRef,
    string ProvenanceRef,
    string Status);
