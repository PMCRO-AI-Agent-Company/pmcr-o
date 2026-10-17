namespace ProjectName.Harness;

using System.Text.Json;
using ProjectName.Contracts;
using ProjectName.Core;

// A reference to signed state. The JSON file itself conveys no authority.
public sealed record CommissioningGrantBoundary(string GrantId, SecretKeyReference KeyReference);

public static class CommissioningGrantAuthorization
{
    public static void Validate(ControlPlaneCommissioningRequest request, IGrantStore grants,
        ICapabilityRegistrationStore registrations, ProductionGrantVerifier verifier)
    {
        var action = request.TrailAction;
        var boundary = JsonSerializer.Deserialize<CommissioningGrantBoundary>(
            File.ReadAllText(action.GrantVerificationRef), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (boundary is null || string.IsNullOrWhiteSpace(boundary.GrantId) || boundary.KeyReference is null)
            throw new UnauthorizedAccessException("signed-commissioning-boundary-required");
        var grant = grants.Get(boundary.GrantId);
        if (grant is null || !verifier.Verify(grant, boundary.KeyReference)
            || grant.CapabilityId != "control-plane.commissioning"
            || grant.Subject != "commissioning:" + action.TrailId)
            throw new UnauthorizedAccessException("commissioning-grant-verification-failed");
        var registration = registrations.Get(grant.RegistrationRef!);
        if (registration is null || registration.Status != "registered"
            || registration.CapabilityId != grant.CapabilityId || registration.CandidateDigest != grant.CandidateDigest
            || registration.CheckerVerdictRef != grant.CheckerVerdictRef || registration.ProvenanceRef != grant.ProvenanceRef)
            throw new UnauthorizedAccessException("commissioning-registration-binding-mismatch");
        var authority = grant.EffectiveGrant ?? [];
        if (!authority.Contains(Permission(request.Operation, action.CapabilityId), StringComparer.Ordinal))
            throw new UnauthorizedAccessException("commissioning-operation-not-granted");
        if (request.Registration is not null && request.Registration.CapabilityId != action.CapabilityId
            || request.GrantRequest is not null && request.GrantRequest.CapabilityId != action.CapabilityId
            || request.ActivationReceipt is not null && request.ActivationReceipt.Grant.CapabilityId != action.CapabilityId)
            throw new UnauthorizedAccessException("commissioning-target-mismatch");
        if (request.GrantRequest is { } derivation)
        {
            // Request-supplied parent ceilings may narrow but never invent delegable authority.
            var allowed = authority.Where(x => x.StartsWith("delegate:" + action.CapabilityId + ":", StringComparison.Ordinal))
                .Select(x => x[("delegate:" + action.CapabilityId + ":").Length..]).ToHashSet(StringComparer.Ordinal);
            if (derivation.ParentAuthority.Any(x => !allowed.Contains(x)) || derivation.Requested.Any(x => !allowed.Contains(x)))
                throw new UnauthorizedAccessException("delegation-exceeds-signed-authority");
        }
    }

    public static string Permission(string operation, string capability) => "control-plane:" + operation + ":" + capability;
}
