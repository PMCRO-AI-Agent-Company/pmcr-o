namespace ProjectName.Harness;

using System.Security.Cryptography;
using System.Text.Json;
using ProjectName.Contracts;
using ProjectName.Core;

public sealed record FirstOperationalGrantRequest(
    LifecycleActionRequest TrailAction, ControlPlaneStatePaths Paths,
    string RegistrationId, string OwnerPrincipal);

// Explicit owner ingress, never accepted as an ordinary commissioning grant boundary.
// The host authenticates the principal; callers cannot supply a different principal to the service.
public sealed class FirstOperationalGrantBootstrap(
    ISecretProvider secrets, ICapabilityRegistrationStore registrations, IGrantStore grants,
    ProductionGrantIssuer issuer, ProductionGrantVerifier verifier, string authenticatedOwner,
    ControlPlaneOperatorTrust operatorTrust)
{
    public const string Operation = "bootstrap-first-operational-grant";
    public const string GrantId = "control-plane.commissioning:first-operational:v1";
    public const string KeyId = "pmcro-control-plane-bootstrap";

    public static readonly IReadOnlyList<string> Permissions = Array.AsReadOnly(new[]
    {
        CommissioningGrantAuthorization.Permission(LifecycleOperations.RegisterCapability, "csuite.executive-council"),
        CommissioningGrantAuthorization.Permission(LifecycleOperations.IssueMinimumGrant, "csuite.executive-council"),
        CommissioningGrantAuthorization.Permission(LifecycleOperations.ActivateCapability, "csuite.executive-council"),
        "delegate:csuite.executive-council:capability.invoke",
        "delegate:csuite.executive-council:csuite.executive-council"
    });

    public ControlPlaneCommissioningResult Run(FirstOperationalGrantRequest request, string mode)
    {
        if (mode is not ("preflight" or "simulate" or "execute" or "check")) throw new ArgumentException("invalid-mode");
        var action = request.TrailAction;
        operatorTrust.ValidatePaths(request.Paths);
        var trustedPaths = operatorTrust.Paths;
        request = request with { Paths = trustedPaths };
        operatorTrust.ValidateBootstrap(action, authenticatedOwner);
        if (string.IsNullOrWhiteSpace(authenticatedOwner) || request.OwnerPrincipal != authenticatedOwner)
            throw new UnauthorizedAccessException("authenticated-owner-required");
        if (action.Operation != Operation || action.CapabilityId != "control-plane.commissioning")
            throw new UnauthorizedAccessException("bootstrap-scope-mismatch");
        var registration = registrations.Get(request.RegistrationId)
            ?? throw new UnauthorizedAccessException("bootstrap-registration-required");
        if (registration.Status != "registered" || registration.CapabilityId != "control-plane.commissioning"
            || registration.CheckerVerdictRef != action.CheckerVerdictRef || registration.ProvenanceRef != action.ProvenanceRef)
            throw new UnauthorizedAccessException("bootstrap-registration-binding-mismatch");
        CommissioningEvidenceValidator.ValidateCommonTrailEvidence(action, registration.CandidateDigest);
        using var authorization = JsonDocument.Parse(File.ReadAllText(action.AuthorizationRef));
        var owner = authorization.RootElement;
        if (owner.GetProperty("operation").GetString() != Operation
            || owner.GetProperty("ownerPrincipal").GetString() != authenticatedOwner
            || owner.GetProperty("trailId").GetString() != action.TrailId
            || owner.GetProperty("seedRef").GetString() != action.SeedRef
            || owner.GetProperty("registrationId").GetString() != registration.RegistrationId
            || owner.GetProperty("candidateDigest").GetString() != registration.CandidateDigest
            || owner.GetProperty("expiresAt").GetDateTimeOffset() <= DateTimeOffset.UtcNow)
            throw new UnauthorizedAccessException("bootstrap-owner-binding-mismatch");
        ValidateCheckedFiles(action.CheckerVerdictRef, registration.FileManifestDigest);

        var key = new SecretKeyReference(KeyId, 1, "windows-dpapi", Path.GetFullPath(request.Paths.ProtectedKeyRoot));
        var state = Path.Combine(Path.GetFullPath(request.Paths.CanonicalRoot), "global", ".pmcro", "bootstrap", "first-operational-grant");
        var consumed = Path.Combine(state, "claimed.json");
        var receipt = Path.Combine(state, "completed.json");
        if (mode == "check")
        {
            var ok = File.Exists(consumed) && File.Exists(receipt) && verifier.Verify(GrantId, key);
            return new(ok, Operation, mode, ok ? "check-pass" : "check-fail", action.TrailId, GrantId: GrantId);
        }
        if (File.Exists(consumed) || secrets.Exists(key) || grants.Get(GrantId) is not null
            || Directory.Exists(request.Paths.GrantRoot) && Directory.EnumerateFiles(request.Paths.GrantRoot, "*.json").Any())
            throw new UnauthorizedAccessException("first-operational-boundary-already-used-or-partial");
        if (mode != "execute") return new(true, Operation, mode, "bootstrap-preflight-pass", action.TrailId);

        // Reserve before any key/grant mutation. A crash consumes the attempt and requires owner recovery.
        Directory.CreateDirectory(state);
        WriteOnce(consumed, new { action.RequestId, action.TrailId, action.SeedRef, request.RegistrationId,
            registration.CandidateDigest, request.OwnerPrincipal, status = "claimed", createdAt = DateTimeOffset.UtcNow });
        var material = RandomNumberGenerator.GetBytes(32);
        try { secrets.Write(key, material); }
        finally { CryptographicOperations.ZeroMemory(material); }
        var grant = issuer.Issue(new GrantDerivationRequest("commissioning:" + action.TrailId,
            registration.RegistrationId, registration.CapabilityId, registration.CandidateDigest,
            Permissions, Permissions, Permissions, Permissions, [], registration.CheckerVerdictRef, registration.ProvenanceRef), key, GrantId);
        if (!verifier.Verify(grant, key)) throw new UnauthorizedAccessException("bootstrap-grant-verification-failed");
        WriteOnce(receipt, new { status = "completed", action.TrailId, action.SeedRef, grant.GrantId,
            grant.RegistrationRef, grant.CandidateDigest, permissions = Permissions, completedAt = DateTimeOffset.UtcNow });
        WriteOnce(Path.Combine(state, "grant-reference.json"), new CommissioningGrantBoundary(GrantId, key));
        return new(true, Operation, mode, "first-operational-grant-issued", action.TrailId,
            registration.RegistrationId, GrantId, EffectiveGrant: grant.EffectiveGrant);
    }

    private static void ValidateCheckedFiles(string checkerPath, string manifest)
    {
        using var checker = JsonDocument.Parse(File.ReadAllText(checkerPath));
        var root = checker.RootElement;
        var checkedAt = root.GetProperty("checkedAt").GetDateTimeOffset();
        if (checkedAt > DateTimeOffset.UtcNow || checkedAt < DateTimeOffset.UtcNow.AddHours(-24))
            throw new UnauthorizedAccessException("fresh-checker-required");
        var files = root.GetProperty("files").Deserialize<Dictionary<string, string>>()!;
        if (files.Count == 0 || AgentActivationGate.ComputeFileManifestDigest(files) != manifest)
            throw new UnauthorizedAccessException("checked-manifest-mismatch");
        foreach (var (path, digest) in files)
        {
            if (!Path.IsPathFullyQualified(path) || !File.Exists(path)) throw new UnauthorizedAccessException("checked-file-missing");
            using var stream = File.OpenRead(path);
            if (!Convert.ToHexString(SHA256.HashData(stream)).Equals(digest, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("checked-file-changed");
        }
    }

    private static void WriteOnce(string path, object value)
    {
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        JsonSerializer.Serialize(stream, value, new JsonSerializerOptions { WriteIndented = true });
        stream.Flush(true);
    }
}
