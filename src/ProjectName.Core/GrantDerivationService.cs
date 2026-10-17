using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ProjectName.Contracts;

namespace ProjectName.Core;

public sealed class GrantDerivationService
{
    private const string CoreIssuer = "pmcro-core/v2";
    private readonly CapabilityRegistrationService _registrations = new();

    public AgentExecutionGrant Derive(
        GrantDerivationRequest request,
        CapabilityRegistration registration,
        string signingKey,
        string grantId) =>
        Derive(request, registration, Encoding.UTF8.GetBytes(signingKey), grantId, null);

    public AgentExecutionGrant Derive(
        GrantDerivationRequest request,
        CapabilityRegistration registration,
        byte[] signingKey,
        string grantId,
        SecretKeyReference? keyReference)
    {
        if (!_registrations.Validate(registration))
            throw new InvalidOperationException("registration-invalid");
        if (!StringComparer.Ordinal.Equals(request.RegistrationRef, registration.RegistrationId))
            throw new InvalidOperationException("registration-mismatch");
        if (!StringComparer.Ordinal.Equals(request.CapabilityId, registration.CapabilityId))
            throw new InvalidOperationException("capability-mismatch");
        if (!StringComparer.OrdinalIgnoreCase.Equals(request.CandidateDigest, registration.CandidateDigest))
            throw new InvalidOperationException("candidate-digest-mismatch");
        if (request.CheckerVerdictRef != registration.CheckerVerdictRef || request.ProvenanceRef != registration.ProvenanceRef)
            throw new InvalidOperationException("provenance-binding-mismatch");
        if (signingKey is null || signingKey.Length == 0)
            throw new InvalidOperationException("core-signing-key-required");

        var effective = request.Requested
            .Intersect(request.ParentAuthority, StringComparer.Ordinal)
            .Intersect(request.HostMaximum, StringComparer.Ordinal)
            .Intersect(request.EnvironmentMaximum, StringComparer.Ordinal)
            .Except(request.Denies, StringComparer.Ordinal)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

        var unsigned = new AgentExecutionGrant(
            effective,
            request.HostMaximum,
            request.EnvironmentMaximum,
            request.Denies,
            "core-derived",
            GrantId: grantId,
            Subject: request.Subject,
            RegistrationRef: request.RegistrationRef,
            CapabilityId: request.CapabilityId,
            CandidateDigest: request.CandidateDigest,
            ParentAuthority: request.ParentAuthority,
            EffectiveGrant: effective,
            Issuer: CoreIssuer,
            IssuerProof: "",
            CheckerVerdictRef: request.CheckerVerdictRef,
            ProvenanceRef: request.ProvenanceRef,
            KeyId: keyReference?.KeyId,
            KeyGeneration: keyReference?.Generation);

        return unsigned with { IssuerProof = Sign(unsigned, signingKey) };
    }

    public bool VerifyProof(AgentExecutionGrant grant, string signingKey) =>
        VerifyProof(grant, Encoding.UTF8.GetBytes(signingKey));

    public bool VerifyProof(AgentExecutionGrant grant, byte[] signingKey)
    {
        if (!StringComparer.Ordinal.Equals(grant.Issuer, CoreIssuer)) return false;
        if (signingKey is null || signingKey.Length == 0 || string.IsNullOrWhiteSpace(grant.IssuerProof)) return false;
        if (grant.EffectiveGrant is null || grant.ParentAuthority is null) return false;
        var expected = Sign(grant with { IssuerProof = "" }, signingKey);
        var left = Convert.FromHexString(expected);
        var right = TryHex(grant.IssuerProof);
        return right is not null && CryptographicOperations.FixedTimeEquals(left, right);
    }

    public static string CanonicalPayload(AgentExecutionGrant grant)
    {
        static string[] Sorted(IEnumerable<string>? values) => (values ?? []).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        // Structured encoding prevents separator collisions; every authority-bearing field is signed.
        return JsonSerializer.Serialize(new
        {
            grant.Issuer, grant.GrantId, grant.Subject, grant.RegistrationRef,
            grant.CapabilityId, grant.CandidateDigest,
            DelegatedGrant = Sorted(grant.DelegatedGrant), EffectiveGrant = Sorted(grant.EffectiveGrant),
            HostMaximum = Sorted(grant.HostMaximum), EnvironmentMaximum = Sorted(grant.EnvironmentMaximum),
            ParentAuthority = Sorted(grant.ParentAuthority), Denies = Sorted(grant.Denies),
            grant.CheckerVerdictRef, grant.ProvenanceRef, grant.KeyId, grant.KeyGeneration, grant.Source
        });
    }

    private static string Sign(AgentExecutionGrant grant, byte[] signingKey)
    {
        using var hmac = new HMACSHA256(signingKey);
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(CanonicalPayload(grant)))).ToLowerInvariant();
    }

    private static byte[]? TryHex(string value)
    {
        try { return Convert.FromHexString(value); }
        catch (FormatException) { return null; }
    }
}
