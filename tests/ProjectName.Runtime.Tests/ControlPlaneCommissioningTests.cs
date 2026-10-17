using System.Text;
using System.Text.Json;
using ProjectName.Contracts;
using ProjectName.Core;
using ProjectName.Harness;
using ProjectName.Infrastructure;
using Xunit;

namespace ProjectName.Runtime.Tests;

public sealed class ControlPlaneCommissioningTests
{
    private static readonly string Candidate = new('a', 64);

    [Fact]
    public async Task Claimed_verified_receipt_cannot_authorize_mutation()
    {
        using var temp = new TempDir();
        var setup = Setup(temp);
        var request = Request(temp, LifecycleOperations.ProvisionKeyGeneration, Candidate,
            key: new SecretKeyReference("new-key", 1, "memory-test", "test"));
        File.WriteAllText(request.TrailAction.GrantVerificationRef, "{\"verified\":true}");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => setup.Service.RunAsync(request, "execute"));
        Assert.False(setup.Secrets.Exists(request.KeyReference!));
    }

    [Theory]
    [InlineData("preflight")]
    [InlineData("simulate")]
    [InlineData("execute")]
    public async Task Revoked_boundary_fails_in_every_mutating_mode(string mode)
    {
        using var temp = new TempDir();
        var setup = Setup(temp);
        var request = Request(temp, LifecycleOperations.ProvisionKeyGeneration, Candidate,
            key: new SecretKeyReference("new-key", 1, "memory-test", "test"));
        setup.Revocations.Save(new RevocationRecord("grant", "boundary-grant", "test", 1, "test"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => setup.Service.RunAsync(request, mode));
        Assert.False(setup.Secrets.Exists(request.KeyReference!));
    }

    [Fact]
    public async Task Cross_trail_boundary_is_rejected()
    {
        using var temp = new TempDir();
        var setup = Setup(temp);
        var request = Request(temp, LifecycleOperations.RegisterCapability, Candidate, registration: Registration("reg-new", Candidate));
        request = request with { TrailAction = request.TrailAction with { TrailId = "other-trail" } };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => setup.Service.RunAsync(request, "execute"));
        Assert.Null(setup.Registrations.Get("reg-new"));
    }

    [Fact]
    public async Task Failed_verification_returns_unsuccessful_result()
    {
        using var temp = new TempDir();
        var setup = Setup(temp);
        var request = Request(temp, "verify-grant", Candidate,
            key: new SecretKeyReference("absent", 1, "memory-test", "test"), grantId: "missing");
        Assert.False((await setup.Service.RunAsync(request, "execute")).Ok);
        Assert.False((await setup.Service.RunAsync(request, "check")).Ok);
    }

    [Fact]
    public void Mutation_rejects_claimed_pass_without_real_evidence_files()
    {
        var action = Action(LifecycleOperations.RegisterCapability, "missing", Candidate);
        Assert.Throws<UnauthorizedAccessException>(() => CommissioningEvidenceValidator.ValidateMutation(action, action.Operation, Candidate));
    }

    [Fact]
    public void Mutation_rejects_unverified_boundary_receipt()
    {
        using var temp = new TempDir();
        var action = EvidenceAction(temp, LifecycleOperations.RegisterCapability, Candidate, grantVerified: false);
        Assert.Throws<UnauthorizedAccessException>(() => CommissioningEvidenceValidator.ValidateMutation(action, action.Operation, Candidate));
    }

    [Fact]
    public async Task Provision_key_is_idempotent_and_never_returns_secret_material()
    {
        using var temp = new TempDir();
        var setup = Setup(temp);
        var key = new SecretKeyReference("commissioning", 1, "memory-test", "test");
        var request = Request(temp, LifecycleOperations.ProvisionKeyGeneration, Candidate, key: key);

        var first = await setup.Service.RunAsync(request, "execute");
        var second = await setup.Service.RunAsync(request, "execute");

        Assert.Equal("provisioned", first.Status);
        Assert.Equal("already-provisioned", second.Status);
        Assert.True(setup.Secrets.Exists(key));
        Assert.DoesNotContain("secret", JsonSerializer.Serialize(first), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Registration_requires_checked_candidate_and_is_idempotent()
    {
        using var temp = new TempDir();
        var setup = Setup(temp);
        var registration = Registration("reg-1", Candidate);
        var request = Request(temp, LifecycleOperations.RegisterCapability, Candidate, registration: registration);

        Assert.Equal("registered", (await setup.Service.RunAsync(request, "execute")).Status);
        Assert.Equal("already-registered", (await setup.Service.RunAsync(request, "execute")).Status);
        Assert.Equal(registration, setup.Registrations.Get("reg-1"));
    }

    [Fact]
    public async Task Minimum_grant_uses_production_issuer_and_verifier()
    {
        using var temp = new TempDir();
        var setup = Setup(temp);
        var key = new SecretKeyReference("commissioning", 1, "memory-test", "test");
        setup.Secrets.Write(key, Encoding.UTF8.GetBytes("test-only-signing-key"));
        var registration = Registration("reg-1", Candidate);
        setup.Registrations.Save(registration);
        var grantRequest = new GrantDerivationRequest(
            "control-plane-commissioner", "reg-1", "control-plane.commissioning", Candidate,
            ["capability.invoke", "control-plane.commissioning"],
            ["capability.invoke", "control-plane.commissioning"],
            ["capability.invoke", "control-plane.commissioning"],
            ["capability.invoke", "control-plane.commissioning"],
            [], registration.CheckerVerdictRef, registration.ProvenanceRef);
        var request = Request(temp, LifecycleOperations.IssueMinimumGrant, Candidate,
            key: key, grantRequest: grantRequest, grantId: "grant-1");

        var result = await setup.Service.RunAsync(request, "execute");
        var repeated = await setup.Service.RunAsync(request, "execute");

        Assert.Equal("issued", result.Status);
        Assert.Equal("already-issued", repeated.Status);
        Assert.True(setup.Verifier.Verify("grant-1", key));
        Assert.DoesNotContain("not-allowed", setup.Grants.Get("grant-1")!.EffectiveGrant!);
    }

    [Fact]
    public async Task Activation_uses_real_gate_and_persists_revalidatable_state()
    {
        using var temp = new TempDir();
        var setup = Setup(temp);
        var key = new SecretKeyReference("commissioning", 1, "memory-test", "test");
        setup.Secrets.Write(key, Encoding.UTF8.GetBytes("activation-test-key"));
        var files = InstalledFiles(temp.Path);
        var registration = Registration("reg-1", Candidate) with
        {
            FileManifestDigest = AgentActivationGate.ComputeFileManifestDigest(files)
        };
        setup.Registrations.Save(registration);
        var grantRequest = new GrantDerivationRequest(
            "control-plane-commissioner", "reg-1", "control-plane.commissioning", Candidate,
            ["capability.invoke"], ["capability.invoke"], ["capability.invoke"], ["capability.invoke"], [],
            registration.CheckerVerdictRef, registration.ProvenanceRef);
        var grant = setup.Issuer.Issue(grantRequest, key, "grant-activation");
        var receipt = new AgentActivationReceipt("PASS", "maker", "checker", Candidate, files, grant);
        var request = Request(temp, LifecycleOperations.ActivateCapability, Candidate,
            key: key, frameworkId: "control-plane", receipt: receipt, ceiling: grant);

        var result = await setup.Service.RunAsync(request, "execute");

        Assert.Equal("activated", result.Status);
        Assert.True(setup.Gate.IsActivated("control-plane"));
        Assert.NotNull(setup.Activations.Get("control-plane"));
    }

    [Fact]
    public async Task Revocation_is_idempotent_and_breaks_grant_verification()
    {
        using var temp = new TempDir();
        var setup = Setup(temp);
        var key = new SecretKeyReference("commissioning", 1, "memory-test", "test");
        setup.Secrets.Write(key, Encoding.UTF8.GetBytes("revocation-test-key"));
        var registration = Registration("reg-1", Candidate);
        setup.Registrations.Save(registration);
        var grantRequest = new GrantDerivationRequest(
            "subject", "reg-1", "control-plane.commissioning", Candidate,
            ["capability.invoke"], ["capability.invoke"], ["capability.invoke"], ["capability.invoke"], [],
            registration.CheckerVerdictRef, registration.ProvenanceRef);
        setup.Issuer.Issue(grantRequest, key, "grant-1");
        var revocation = new RevocationRecord("grant", "grant-1", "test", 1, registration.ProvenanceRef);
        var request = Request(temp, LifecycleOperations.Revoke, Candidate, revocation: revocation);

        Assert.Equal("revoked", (await setup.Service.RunAsync(request, "execute")).Status);
        Assert.Equal("already-revoked", (await setup.Service.RunAsync(request, "execute")).Status);
        Assert.False(setup.Verifier.Verify("grant-1", key));
    }

    [Fact]
    public async Task Inspect_state_is_read_only_and_non_authoritative()
    {
        using var temp = new TempDir();
        var setup = Setup(temp);
        var action = new LifecycleActionRequest(
            "request", LifecycleOperations.InspectState, "trail", "seed", "cause", "corr",
            "", "", "", "", "", "control-plane.commissioning", "event", "", false);
        var request = new ControlPlaneCommissioningRequest(
            LifecycleOperations.InspectState, action, Paths(temp));

        var result = await setup.Service.RunAsync(request, "execute");

        Assert.Equal("inspected", result.Status);
        Assert.False(result.GrantsAuthority);
    }

    [Fact]
    public async Task Legacy_first_registration_boundary_cannot_bypass_signed_authority()
    {
        using var temp = new TempDir();
        var setup = Setup(temp);
        var registration = Registration("reg-first", Candidate);
        var checker = temp.Write("checker.json", JsonSerializer.Serialize(new
        {
            verdict = "PASS", independent = true, makerIdentity = "maker", checkerIdentity = "checker", candidateDigest = Candidate
        }));
        var authorization = temp.Write("authorization.json", "{\"status\":\"open\"}");
        var admission = temp.Write("admission.json", "{\"admitted\":true}");
        var provenance = temp.Write("provenance.json", "{\"trailId\":\"trail\"}");
        var consumed = Path.Combine(temp.Path, "boundary.consumed.json");
        var boundary = temp.Write("boundary.json", JsonSerializer.Serialize(new
        {
            kind = "FirstRegistrationBoundary", status = "open", ownerDerived = true, singleUse = true,
            nonDelegable = true, nonExpandable = true, trailId = "trail", seedRef = "seed",
            operation = LifecycleOperations.RegisterCapability, capabilityId = "control-plane.commissioning",
            candidateDigest = Candidate, checkerVerdictRef = checker, allowedRegistrationCount = 1,
            grantsOrdinaryRuntimeAuthority = false, grantsActivationAuthority = false, grantsExecutionAuthority = false,
            consumptionReceiptRef = consumed
        }));
        var action = new LifecycleActionRequest(
            "request", LifecycleOperations.RegisterCapability, "trail", "seed", "cause", "corr",
            authorization, checker, provenance, admission, boundary, "control-plane.commissioning", "event", "PASS", false);
        var request = new ControlPlaneCommissioningRequest(
            LifecycleOperations.RegisterCapability, action, Paths(temp), Registration: registration);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => setup.Service.RunAsync(request, "execute"));
        Assert.Null(setup.Registrations.Get("reg-first"));
        Assert.False(File.Exists(consumed));
    }

    [Fact]
    public void First_registration_boundary_rejects_different_capability()
    {
        using var temp = new TempDir();
        var checker = temp.Write("checker.json", JsonSerializer.Serialize(new
        {
            verdict = "PASS", independent = true, makerIdentity = "maker", checkerIdentity = "checker", candidateDigest = Candidate
        }));
        var authorization = temp.Write("authorization.json", "{\"status\":\"open\"}");
        var admission = temp.Write("admission.json", "{\"admitted\":true}");
        var provenance = temp.Write("provenance.json", "{\"trailId\":\"trail\"}");
        var boundary = temp.Write("boundary.json", JsonSerializer.Serialize(new
        {
            kind = "FirstRegistrationBoundary", status = "open", ownerDerived = true, singleUse = true,
            nonDelegable = true, nonExpandable = true, trailId = "trail", seedRef = "seed",
            operation = LifecycleOperations.RegisterCapability, capabilityId = "control-plane.commissioning",
            candidateDigest = Candidate, checkerVerdictRef = checker, allowedRegistrationCount = 1,
            grantsOrdinaryRuntimeAuthority = false, grantsActivationAuthority = false, grantsExecutionAuthority = false,
            consumptionReceiptRef = Path.Combine(temp.Path, "consumed.json")
        }));
        var action = new LifecycleActionRequest(
            "request", LifecycleOperations.RegisterCapability, "trail", "seed", "cause", "corr",
            authorization, checker, provenance, admission, boundary, "other.capability", "event", "PASS", false);

        Assert.Throws<UnauthorizedAccessException>(() =>
            CommissioningEvidenceValidator.ValidateMutation(action, LifecycleOperations.RegisterCapability, Candidate));
    }

    private static CapabilityRegistration Registration(string id, string digest) => new(
        id, "control-plane.commissioning", digest, new string('b', 64), "checker-ref", "provenance-ref", "registered");

    private static ControlPlaneCommissioningRequest Request(
        TempDir temp,
        string operation,
        string digest,
        SecretKeyReference? key = null,
        CapabilityRegistration? registration = null,
        GrantDerivationRequest? grantRequest = null,
        string? grantId = null,
        string? frameworkId = null,
        AgentActivationReceipt? receipt = null,
        AgentExecutionGrant? ceiling = null,
        RevocationRecord? revocation = null) =>
        new(operation, EvidenceAction(temp, operation, digest), Paths(temp), key, registration, grantRequest,
            grantId, frameworkId, receipt, ceiling, revocation);

    private static LifecycleActionRequest EvidenceAction(TempDir temp, string operation, string digest, bool grantVerified = true)
    {
        var authorization = temp.Write("authorization.json", "{\"status\":\"open\"}");
        var checker = temp.Write("checker.json", JsonSerializer.Serialize(new
        {
            verdict = "PASS", independent = true, makerIdentity = "maker", checkerIdentity = "checker", candidateDigest = digest
        }));
        var admission = temp.Write("admission.json", "{\"Admitted\":true}");
        var provenance = temp.Write("provenance.json", "{\"trail\":\"trail\"}");
        var grant = temp.Write("grant-verification.json", grantVerified
            ? JsonSerializer.Serialize(new CommissioningGrantBoundary("boundary-grant", new SecretKeyReference("boundary-key", 1, "memory-test", "test")))
            : "{\"verified\":false}");
        return new LifecycleActionRequest(
            "request", operation, "trail", "seed", "cause", "corr", authorization, checker, provenance,
            admission, grant, "control-plane.commissioning", "event", "PASS", grantVerified);
    }

    private static LifecycleActionRequest Action(string operation, string root, string digest) => new(
        "request", operation, "trail", "seed", "cause", "corr",
        Path.Combine(root, "authorization.json"), Path.Combine(root, "checker.json"), Path.Combine(root, "provenance.json"),
        Path.Combine(root, "admission.json"), Path.Combine(root, "grant.json"), "control-plane.commissioning", "event", "PASS", true);

    private static ControlPlaneStatePaths Paths(TempDir temp) => new(
        temp.Path, temp.Sub("registrations"), temp.Sub("grants"), temp.Sub("revocations"), temp.Sub("activations"), temp.Sub("keys"));

    private static SetupState Setup(TempDir temp)
    {
        var secrets = new MemorySecretProvider();
        var registrations = new FileCapabilityRegistrationStore(temp.Sub("registrations"));
        var grants = new FileGrantStore(temp.Sub("grants"));
        var revocations = new FileRevocationStore(temp.Sub("revocations"));
        var activations = new FileActivationStore(temp.Sub("activations"));
        var issuer = new ProductionGrantIssuer(secrets, registrations, grants, revocations);
        var verifier = new ProductionGrantVerifier(secrets, grants, revocations);
        var boundaryKey = new SecretKeyReference("boundary-key", 1, "memory-test", "test");
        secrets.Write(boundaryKey, Encoding.UTF8.GetBytes("synthetic-test-boundary-key"));
        var boundaryRegistration = Registration("boundary-reg", Candidate);
        registrations.Save(boundaryRegistration);
        var permissions = LifecycleOperations.Mutating.Select(x => CommissioningGrantAuthorization.Permission(x, "control-plane.commissioning"))
            .Concat(new[] { "delegate:control-plane.commissioning:capability.invoke", "delegate:control-plane.commissioning:control-plane.commissioning" }).ToArray();
        issuer.Issue(new GrantDerivationRequest("commissioning:trail", "boundary-reg", "control-plane.commissioning", Candidate,
            permissions, permissions, permissions, permissions, [], boundaryRegistration.CheckerVerdictRef, boundaryRegistration.ProvenanceRef), boundaryKey, "boundary-grant");
        var gate = new AgentActivationGate(verifier, registrations, revocations, activations, "memory-test", "test");
        var service = new ControlPlaneCommissioningService(secrets, registrations, grants, revocations, issuer, verifier, gate);
        return new SetupState(secrets, registrations, grants, revocations, activations, issuer, verifier, gate, service);
    }

    private sealed record SetupState(
        MemorySecretProvider Secrets,
        FileCapabilityRegistrationStore Registrations,
        FileGrantStore Grants,
        FileRevocationStore Revocations,
        FileActivationStore Activations,
        ProductionGrantIssuer Issuer,
        ProductionGrantVerifier Verifier,
        AgentActivationGate Gate,
        ControlPlaneCommissioningService Service);

    private static Dictionary<string, string> InstalledFiles(string root)
    {
        var path = System.IO.Path.Combine(root, "candidate.dll");
        System.IO.File.WriteAllText(path, "synthetic candidate bytes");
        return new() { [path] = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.IO.File.ReadAllBytes(path))).ToLowerInvariant() };
    }
    private sealed class MemorySecretProvider : ISecretProvider
    {
        private readonly Dictionary<string, byte[]> _values = new(StringComparer.Ordinal);
        private static string Id(SecretKeyReference reference) => reference.KeyId + ":" + reference.Generation;
        public bool Exists(SecretKeyReference reference) => _values.ContainsKey(Id(reference));
        public byte[] Read(SecretKeyReference reference) => _values[Id(reference)].ToArray();
        public void Write(SecretKeyReference reference, byte[] value) => _values[Id(reference)] = value.ToArray();
    }

    private sealed class TempDir : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "pmcro-commissioning-" + Guid.NewGuid().ToString("N"));
        public TempDir() => Directory.CreateDirectory(Path);
        public string Sub(string name) { var p = System.IO.Path.Combine(Path, name); Directory.CreateDirectory(p); return p; }
        public string Write(string name, string text) { var p = System.IO.Path.Combine(Path, name); File.WriteAllText(p, text); return p; }
        public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, true); }
    }
}
