namespace ProjectName.Contracts;

public sealed record SecretKeyReference(string KeyId, int Generation, string Provider, string LocationRef);

public sealed record RevocationRecord(
    string TargetType,
    string TargetId,
    string Reason,
    long Sequence,
    string ProvenanceRef);

public interface ISecretProvider
{
    bool Exists(SecretKeyReference reference);
    byte[] Read(SecretKeyReference reference);
    void Write(SecretKeyReference reference, byte[] value);
}

public interface ICapabilityRegistrationStore
{
    CapabilityRegistration? Get(string registrationId);
    void Save(CapabilityRegistration registration);
}

public interface IGrantStore
{
    AgentExecutionGrant? Get(string grantId);
    void Save(AgentExecutionGrant grant);
}

public interface IRevocationStore
{
    bool IsRevoked(string targetType, string targetId);
    void Save(RevocationRecord revocation);
}

public sealed record ActivationRecord(
    string FrameworkId,
    string Status,
    string ReceiptDigest,
    string GrantId,
    string RegistrationRef,
    string CandidateDigest,
    IReadOnlyList<string> EffectiveGrant,
    string CheckerIdentity,
    string ProvenanceRef,
    string KeyId,
    int KeyGeneration,
    DateTimeOffset ActivatedAt,
    IReadOnlyDictionary<string, string>? Files = null,
    string? MakerIdentity = null);

public interface IActivationStore
{
    ActivationRecord? Get(string frameworkId);
    void Save(ActivationRecord activation);
}
