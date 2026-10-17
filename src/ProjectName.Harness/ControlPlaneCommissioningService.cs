namespace ProjectName.Harness;

using System.Security.Cryptography;
using ProjectName.Contracts;
using ProjectName.Core;

public sealed class ControlPlaneCommissioningService(
    ISecretProvider secrets,
    ICapabilityRegistrationStore registrations,
    IGrantStore grants,
    IRevocationStore revocations,
    ProductionGrantIssuer issuer,
    ProductionGrantVerifier verifier,
    IFrameworkActivation activation)
{
    private static readonly IReadOnlySet<string> Operations = new HashSet<string>(StringComparer.Ordinal)
    {
        LifecycleOperations.ProvisionKeyGeneration,
        LifecycleOperations.RegisterCapability,
        LifecycleOperations.IssueMinimumGrant,
        "verify-grant",
        LifecycleOperations.ActivateCapability,
        LifecycleOperations.Revoke,
        LifecycleOperations.InspectState
    };

    public static void ValidatePreflight(ControlPlaneCommissioningRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!Operations.Contains(request.Operation)) throw new ArgumentException("unknown-control-plane-operation");
        ValidateShape(request);
        if (request.TrailAction.RequestsMutation)
            CommissioningEvidenceValidator.ValidateMutation(request.TrailAction, request.Operation, ExpectedCandidateDigest(request));
    }

    public async Task<ControlPlaneCommissioningResult> RunAsync(
        ControlPlaneCommissioningRequest request,
        string mode,
        CancellationToken ct = default)
    {
        if (mode is not ("preflight" or "simulate" or "execute" or "check")) throw new ArgumentException("unknown-control-plane-mode");
        ValidatePreflight(request);
        if (request.TrailAction.RequestsMutation)
            CommissioningGrantAuthorization.Validate(request, grants, registrations, verifier);

        if (mode == "preflight")
            return Result(request, mode, "preflight-pass");
        if (mode == "simulate")
            return Simulate(request, mode);
        if (mode == "check")
            return Check(request, mode);

        return request.Operation switch
        {
            LifecycleOperations.ProvisionKeyGeneration => ProvisionKey(request, mode),
            LifecycleOperations.RegisterCapability => Register(request, mode),
            LifecycleOperations.IssueMinimumGrant => IssueGrant(request, mode),
            "verify-grant" => VerifyGrant(request, mode),
            LifecycleOperations.ActivateCapability => await ActivateAsync(request, mode, ct),
            LifecycleOperations.Revoke => Revoke(request, mode),
            LifecycleOperations.InspectState => Inspect(request, mode),
            _ => throw new ArgumentException("unknown-control-plane-operation")
        };
    }

    private ControlPlaneCommissioningResult ProvisionKey(ControlPlaneCommissioningRequest request, string mode)
    {
        var key = request.KeyReference!;
        if (secrets.Exists(key)) return Result(request, mode, "already-provisioned");
        var material = RandomNumberGenerator.GetBytes(32);
        try { secrets.Write(key, material); }
        finally { Array.Clear(material, 0, material.Length); }
        return Result(request, mode, "provisioned");
    }

    private ControlPlaneCommissioningResult Register(ControlPlaneCommissioningRequest request, string mode)
    {
        var registration = request.Registration!;
        if (!new CapabilityRegistrationService().Validate(registration))
            throw new InvalidOperationException("registration-invalid");
        var existing = registrations.Get(registration.RegistrationId);
        if (existing is not null)
        {
            if (existing == registration) return Result(request, mode, "already-registered", registrationId: registration.RegistrationId);
            throw new InvalidOperationException("registration-id-conflict");
        }
        registrations.Save(registration);
        return Result(request, mode, "registered", registrationId: registration.RegistrationId);
    }

    private ControlPlaneCommissioningResult IssueGrant(ControlPlaneCommissioningRequest request, string mode)
    {
        var grantId = request.GrantId!;
        var key = request.KeyReference!;
        var existing = grants.Get(grantId);
        if (existing is not null)
        {
            if (StringComparer.Ordinal.Equals(existing.CapabilityId, request.GrantRequest!.CapabilityId)
                && StringComparer.OrdinalIgnoreCase.Equals(existing.CandidateDigest, request.GrantRequest.CandidateDigest)
                && verifier.Verify(grantId, key))
                return Result(request, mode, "already-issued", grantId: grantId, effective: existing.EffectiveGrant);
            throw new InvalidOperationException("grant-id-conflict");
        }
        var grant = issuer.Issue(request.GrantRequest!, key, grantId);
        return Result(request, mode, "issued", grantId: grantId, effective: grant.EffectiveGrant);
    }

    private ControlPlaneCommissioningResult VerifyGrant(ControlPlaneCommissioningRequest request, string mode)
    {
        var verified = verifier.Verify(request.GrantId!, request.KeyReference!);
        return Result(request, mode, verified ? "verified" : "not-verified", grantId: request.GrantId,
            evidence: new Dictionary<string, string> { ["verified"] = verified ? "true" : "false" });
    }

    private async Task<ControlPlaneCommissioningResult> ActivateAsync(ControlPlaneCommissioningRequest request, string mode, CancellationToken ct)
    {
        var effective = await activation.ActivateAsync(request.FrameworkId!, request.ActivationReceipt!, request.ActivationCeiling!, ct);
        return Result(request, mode, "activated", grantId: effective.GrantId, frameworkId: request.FrameworkId, effective: effective.EffectiveGrant);
    }

    private ControlPlaneCommissioningResult Revoke(ControlPlaneCommissioningRequest request, string mode)
    {
        var revocation = request.Revocation!;
        if (revocations.IsRevoked(revocation.TargetType, revocation.TargetId))
            return Result(request, mode, "already-revoked");
        revocations.Save(revocation);
        return Result(request, mode, "revoked");
    }

    private ControlPlaneCommissioningResult Inspect(ControlPlaneCommissioningRequest request, string mode) =>
        Result(request, mode, "inspected", request.Registration?.RegistrationId, request.GrantId, request.FrameworkId,
            evidence: InspectEvidence(request));

    private ControlPlaneCommissioningResult Simulate(ControlPlaneCommissioningRequest request, string mode)
    {
        var status = request.Operation switch
        {
            LifecycleOperations.ProvisionKeyGeneration => secrets.Exists(request.KeyReference!) ? "already-provisioned" : "would-provision",
            LifecycleOperations.RegisterCapability => registrations.Get(request.Registration!.RegistrationId) is null ? "would-register" : "registration-exists",
            LifecycleOperations.IssueMinimumGrant => grants.Get(request.GrantId!) is null ? "would-issue" : "grant-exists",
            "verify-grant" => verifier.Verify(request.GrantId!, request.KeyReference!) ? "verified" : "not-verified",
            LifecycleOperations.ActivateCapability => activation.IsActivated(request.FrameworkId!) ? "already-active" : "would-activate",
            LifecycleOperations.Revoke => revocations.IsRevoked(request.Revocation!.TargetType, request.Revocation.TargetId) ? "already-revoked" : "would-revoke",
            LifecycleOperations.InspectState => "would-inspect",
            _ => "unsupported"
        };
        return Result(request, mode, status, evidence: request.Operation == LifecycleOperations.InspectState ? InspectEvidence(request) : null);
    }

    private ControlPlaneCommissioningResult Check(ControlPlaneCommissioningRequest request, string mode)
    {
        var ok = request.Operation switch
        {
            LifecycleOperations.ProvisionKeyGeneration => secrets.Exists(request.KeyReference!),
            LifecycleOperations.RegisterCapability => registrations.Get(request.Registration!.RegistrationId) == request.Registration,
            LifecycleOperations.IssueMinimumGrant => verifier.Verify(request.GrantId!, request.KeyReference!),
            "verify-grant" => verifier.Verify(request.GrantId!, request.KeyReference!),
            LifecycleOperations.ActivateCapability => activation.IsActivated(request.FrameworkId!),
            LifecycleOperations.Revoke => revocations.IsRevoked(request.Revocation!.TargetType, request.Revocation.TargetId),
            LifecycleOperations.InspectState => true,
            _ => false
        };
        return Result(request, mode, ok ? "check-pass" : "check-fail", evidence: InspectEvidence(request));
    }

    private IReadOnlyDictionary<string, string> InspectEvidence(ControlPlaneCommissioningRequest request)
    {
        var evidence = new Dictionary<string, string>(StringComparer.Ordinal);
        if (request.KeyReference is not null) evidence["keyExists"] = secrets.Exists(request.KeyReference) ? "true" : "false";
        if (request.Registration is not null) evidence["registrationExists"] = registrations.Get(request.Registration.RegistrationId) is not null ? "true" : "false";
        if (!string.IsNullOrWhiteSpace(request.GrantId) && request.KeyReference is not null) evidence["grantVerified"] = verifier.Verify(request.GrantId, request.KeyReference) ? "true" : "false";
        if (!string.IsNullOrWhiteSpace(request.FrameworkId)) evidence["activated"] = activation.IsActivated(request.FrameworkId) ? "true" : "false";
        if (request.Revocation is not null) evidence["revoked"] = revocations.IsRevoked(request.Revocation.TargetType, request.Revocation.TargetId) ? "true" : "false";
        return evidence;
    }

    private static void ValidateShape(ControlPlaneCommissioningRequest request)
    {
        if (!StringComparer.Ordinal.Equals(request.Operation, request.TrailAction.Operation))
            throw new ArgumentException("operation-trail-action-mismatch");
        switch (request.Operation)
        {
            case LifecycleOperations.ProvisionKeyGeneration:
                if (request.KeyReference is null) throw new ArgumentException("key-reference-required");
                break;
            case LifecycleOperations.RegisterCapability:
                if (request.Registration is null) throw new ArgumentException("registration-required");
                break;
            case LifecycleOperations.IssueMinimumGrant:
                if (request.GrantRequest is null || request.KeyReference is null || string.IsNullOrWhiteSpace(request.GrantId))
                    throw new ArgumentException("grant-request-key-and-id-required");
                break;
            case "verify-grant":
                if (request.KeyReference is null || string.IsNullOrWhiteSpace(request.GrantId)) throw new ArgumentException("grant-id-and-key-required");
                break;
            case LifecycleOperations.ActivateCapability:
                if (string.IsNullOrWhiteSpace(request.FrameworkId) || request.ActivationReceipt is null || request.ActivationCeiling is null || request.KeyReference is null)
                    throw new ArgumentException("framework-receipt-ceiling-and-key-required");
                break;
            case LifecycleOperations.Revoke:
                if (request.Revocation is null) throw new ArgumentException("revocation-required");
                break;
        }
    }

    private static string? ExpectedCandidateDigest(ControlPlaneCommissioningRequest request) => request.Operation switch
    {
        LifecycleOperations.RegisterCapability => request.Registration?.CandidateDigest,
        LifecycleOperations.IssueMinimumGrant => request.GrantRequest?.CandidateDigest,
        LifecycleOperations.ActivateCapability => request.ActivationReceipt?.CandidateDigest,
        _ => null
    };

    private static ControlPlaneCommissioningResult Result(
        ControlPlaneCommissioningRequest request,
        string mode,
        string status,
        string? registrationId = null,
        string? grantId = null,
        string? frameworkId = null,
        IReadOnlyList<string>? effective = null,
        IReadOnlyDictionary<string, string>? evidence = null) =>
        new(status is not ("check-fail" or "not-verified"), request.Operation, mode, status, request.TrailAction.TrailId, registrationId, grantId, frameworkId, effective, evidence);
}
