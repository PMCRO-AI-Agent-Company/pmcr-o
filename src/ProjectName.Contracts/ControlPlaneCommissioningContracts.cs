namespace ProjectName.Contracts;

public sealed record ControlPlaneStatePaths(
    string CanonicalRoot,
    string RegistrationRoot,
    string GrantRoot,
    string RevocationRoot,
    string ActivationRoot,
    string ProtectedKeyRoot);

public sealed record ControlPlaneCommissioningRequest(
    string Operation,
    LifecycleActionRequest TrailAction,
    ControlPlaneStatePaths Paths,
    SecretKeyReference? KeyReference = null,
    CapabilityRegistration? Registration = null,
    GrantDerivationRequest? GrantRequest = null,
    string? GrantId = null,
    string? FrameworkId = null,
    AgentActivationReceipt? ActivationReceipt = null,
    AgentExecutionGrant? ActivationCeiling = null,
    RevocationRecord? Revocation = null);

public sealed record ControlPlaneCommissioningResult(
    bool Ok,
    string Operation,
    string Mode,
    string Status,
    string TrailId,
    string? RegistrationId = null,
    string? GrantId = null,
    string? FrameworkId = null,
    IReadOnlyList<string>? EffectiveGrant = null,
    IReadOnlyDictionary<string, string>? Evidence = null)
{
    public bool GrantsAuthority => false;
}
