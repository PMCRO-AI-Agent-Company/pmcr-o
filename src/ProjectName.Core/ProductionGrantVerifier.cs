using ProjectName.Contracts;

namespace ProjectName.Core;

public sealed class ProductionGrantVerifier
{
    private readonly ISecretProvider _secrets;
    private readonly IGrantStore _grants;
    private readonly IRevocationStore _revocations;
    private readonly GrantDerivationService _derivation;

    public ProductionGrantVerifier(
        ISecretProvider secrets,
        IGrantStore grants,
        IRevocationStore revocations,
        GrantDerivationService? derivation = null)
    {
        _secrets = secrets;
        _grants = grants;
        _revocations = revocations;
        _derivation = derivation ?? new GrantDerivationService();
    }

    public bool Verify(string grantId, SecretKeyReference keyReference)
    {
        var grant = _grants.Get(grantId);
        if (grant is null) return false;
        if (!StringComparer.Ordinal.Equals(grant.GrantId, grantId)) return false;
        if (!StringComparer.Ordinal.Equals(grant.KeyId, keyReference.KeyId)) return false;
        if (grant.KeyGeneration != keyReference.Generation) return false;
        if (_revocations.IsRevoked("grant", grantId)) return false;
        if (!string.IsNullOrWhiteSpace(grant.RegistrationRef) && _revocations.IsRevoked("registration", grant.RegistrationRef)) return false;
        if (_revocations.IsRevoked("key", ProductionGrantIssuer.KeyTarget(keyReference))) return false;
        if (!_secrets.Exists(keyReference)) return false;

        var key = _secrets.Read(keyReference);
        try { return _derivation.VerifyProof(grant, key); }
        finally { Array.Clear(key, 0, key.Length); }
    }

    // Verify the exact value being consumed, not just another record with the same ID.
    public bool Verify(AgentExecutionGrant grant, SecretKeyReference keyReference)
    {
        if (string.IsNullOrWhiteSpace(grant.GrantId) || !Verify(grant.GrantId, keyReference)) return false;
        var stored = _grants.Get(grant.GrantId);
        return stored is not null
            && StringComparer.Ordinal.Equals(GrantDerivationService.CanonicalPayload(stored), GrantDerivationService.CanonicalPayload(grant))
            && StringComparer.Ordinal.Equals(stored.IssuerProof, grant.IssuerProof);
    }

    public AgentExecutionGrant? GetVerified(string grantId, SecretKeyReference keyReference)
    {
        var grant = _grants.Get(grantId);
        return grant is not null && Verify(grant, keyReference) ? grant : null;
    }
}
