using ProjectName.Contracts;

namespace ProjectName.Core;

public sealed class ProductionGrantIssuer
{
    private readonly ISecretProvider _secrets;
    private readonly ICapabilityRegistrationStore _registrations;
    private readonly IGrantStore _grants;
    private readonly IRevocationStore _revocations;
    private readonly GrantDerivationService _derivation;

    public ProductionGrantIssuer(
        ISecretProvider secrets,
        ICapabilityRegistrationStore registrations,
        IGrantStore grants,
        IRevocationStore revocations,
        GrantDerivationService? derivation = null)
    {
        _secrets = secrets;
        _registrations = registrations;
        _grants = grants;
        _revocations = revocations;
        _derivation = derivation ?? new GrantDerivationService();
    }

    public AgentExecutionGrant Issue(
        GrantDerivationRequest request,
        SecretKeyReference keyReference,
        string grantId)
    {
        var registration = _registrations.Get(request.RegistrationRef)
            ?? throw new InvalidOperationException("registration-not-found");

        if (_revocations.IsRevoked("registration", registration.RegistrationId))
            throw new InvalidOperationException("registration-revoked");
        if (_revocations.IsRevoked("key", KeyTarget(keyReference)))
            throw new InvalidOperationException("key-generation-revoked");
        if (!_secrets.Exists(keyReference))
            throw new InvalidOperationException("key-material-unavailable");

        var key = _secrets.Read(keyReference);
        try
        {
            var grant = _derivation.Derive(request, registration, key, grantId, keyReference);
            _grants.Save(grant);
            return grant;
        }
        finally
        {
            Array.Clear(key, 0, key.Length);
        }
    }

    public static string KeyTarget(SecretKeyReference reference) => reference.KeyId + ":" + reference.Generation;
}
