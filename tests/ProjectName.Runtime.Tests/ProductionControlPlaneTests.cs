using System.Text;
using ProjectName.Contracts;
using ProjectName.Core;
using ProjectName.Infrastructure;
using ProjectName.Harness;
using Xunit;

namespace ProjectName.Runtime.Tests;

public class ProductionControlPlaneTests
{
    [Fact]
    public void Signed_payload_has_no_delimiter_collision_and_binds_delegation()
    {
        var derivation = new GrantDerivationService();
        var grant = derivation.Derive(Request, Registration, "test-key", "test-grant");
        Assert.False(derivation.VerifyProof(grant with { DelegatedGrant = ["admin"] }, "test-key"));
        Assert.NotEqual(GrantDerivationService.CanonicalPayload(grant with { EffectiveGrant = ["a,b"] }),
            GrantDerivationService.CanonicalPayload(grant with { EffectiveGrant = ["a", "b"] }));
        Assert.NotEqual(GrantDerivationService.CanonicalPayload(grant with { Subject = "a|b", RegistrationRef = "c" }),
            GrantDerivationService.CanonicalPayload(grant with { Subject = "a", RegistrationRef = "b|c" }));
    }

    [Fact]
    public async Task Activation_rejects_tampered_inline_grant_even_with_valid_stored_id()
    {
        using var temp = new TempDir();
        var secrets = new MemorySecretProvider();
        var key = new SecretKeyReference("key", 1, "memory-test", "test");
        secrets.Write(key, Encoding.UTF8.GetBytes("synthetic-test-key"));
        var regs = new FileCapabilityRegistrationStore(Path.Combine(temp.Path, "r"));
        var grants = new FileGrantStore(Path.Combine(temp.Path, "g"));
        var revocations = new FileRevocationStore(Path.Combine(temp.Path, "x"));
        var activations = new FileActivationStore(Path.Combine(temp.Path, "a"));
        var files = InstalledFiles(temp.Path);
        regs.Save(Registration with { FileManifestDigest = AgentActivationGate.ComputeFileManifestDigest(files) });
        var grant = new ProductionGrantIssuer(secrets, regs, grants, revocations).Issue(Request, key, "g");
        var forged = grant with { EffectiveGrant = ["admin"], DelegatedGrant = ["admin"] };
        var gate = new AgentActivationGate(new ProductionGrantVerifier(secrets, grants, revocations), regs, revocations, activations, "memory-test", "test");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => gate.ActivateAsync("csuite",
            new AgentActivationReceipt("PASS", "maker", "checker", grant.CandidateDigest!, files, forged), forged));
        Assert.Null(activations.Get("csuite"));
        var receipt = new AgentActivationReceipt("PASS", "maker", "checker", grant.CandidateDigest!, files, grant);
        var narrowed = await gate.ActivateAsync("csuite", receipt, grant with { Denies = ["capability.invoke"], HostMaximum = ["capability.invoke"] });
        Assert.Empty(narrowed.EffectiveGrant!);
    }

    [Theory]
    [InlineData("../escape")]
    [InlineData("C:alternate")]
    [InlineData("a/b")]
    public void Dpapi_rejects_unsafe_key_names(string keyId)
    {
        if (!OperatingSystem.IsWindows()) return;
        using var temp = new TempDir();
        var provider = new WindowsDpapiSecretProvider(Path.Combine(temp.Path, "keys"), Path.Combine(temp.Path, "canonical"));
        Assert.Throws<InvalidOperationException>(() => provider.Write(new SecretKeyReference(keyId, 1, "windows-dpapi", "test"), [1, 2, 3]));
        Assert.False(Directory.Exists(Path.Combine(temp.Path, "keys")));
    }

    private static CapabilityRegistration Registration => new(
        "reg-1", "example.install", new string('a', 64), new string('b', 64), "checker:pass", "trail:T1", "registered");

    private static GrantDerivationRequest Request => new(
        "pmcro-maker", "reg-1", "example.install", new string('a', 64),
        ["capability.invoke", "example.install", "extra"],
        ["capability.invoke", "example.install"],
        ["capability.invoke", "example.install"],
        ["capability.invoke", "example.install"],
        [], "checker:pass", "trail:T1");

    [Fact]
    public void Production_grant_binds_key_generation_and_verifies()
    {
        using var temp = new TempDir();
        var secrets = new MemorySecretProvider();
        var keyRef = new SecretKeyReference("pmcro-core-grant-signing", 1, "memory-test", "test");
        secrets.Write(keyRef, Encoding.UTF8.GetBytes("test-only-key"));
        var regs = new FileCapabilityRegistrationStore(Path.Combine(temp.Path, "registrations"));
        var grants = new FileGrantStore(Path.Combine(temp.Path, "grants"));
        var revocations = new FileRevocationStore(Path.Combine(temp.Path, "revocations"));
        regs.Save(Registration);

        var issuer = new ProductionGrantIssuer(secrets, regs, grants, revocations);
        var grant = issuer.Issue(Request, keyRef, "grant-1");

        Assert.Equal(keyRef.KeyId, grant.KeyId);
        Assert.Equal(keyRef.Generation, grant.KeyGeneration);
        Assert.DoesNotContain("extra", grant.EffectiveGrant!);
        Assert.True(new ProductionGrantVerifier(secrets, grants, revocations).Verify("grant-1", keyRef));
    }

    [Fact]
    public void Wrong_key_generation_cannot_verify_existing_grant()
    {
        using var temp = new TempDir();
        var secrets = new MemorySecretProvider();
        var key1 = new SecretKeyReference("signing", 1, "memory-test", "test");
        var key2 = new SecretKeyReference("signing", 2, "memory-test", "test");
        secrets.Write(key1, Encoding.UTF8.GetBytes("key-one"));
        secrets.Write(key2, Encoding.UTF8.GetBytes("key-two"));
        var regs = new FileCapabilityRegistrationStore(Path.Combine(temp.Path, "r"));
        var grants = new FileGrantStore(Path.Combine(temp.Path, "g"));
        var revocations = new FileRevocationStore(Path.Combine(temp.Path, "x"));
        regs.Save(Registration);
        new ProductionGrantIssuer(secrets, regs, grants, revocations).Issue(Request, key1, "grant-1");

        Assert.False(new ProductionGrantVerifier(secrets, grants, revocations).Verify("grant-1", key2));
    }

    [Fact]
    public void Grant_registration_and_key_revocations_fail_closed()
    {
        using var temp = new TempDir();
        var secrets = new MemorySecretProvider();
        var keyRef = new SecretKeyReference("signing", 1, "memory-test", "test");
        secrets.Write(keyRef, Encoding.UTF8.GetBytes("test-key"));
        var regs = new FileCapabilityRegistrationStore(Path.Combine(temp.Path, "r"));
        var grants = new FileGrantStore(Path.Combine(temp.Path, "g"));
        var revocations = new FileRevocationStore(Path.Combine(temp.Path, "x"));
        regs.Save(Registration);
        var issuer = new ProductionGrantIssuer(secrets, regs, grants, revocations);
        issuer.Issue(Request, keyRef, "grant-1");
        var verifier = new ProductionGrantVerifier(secrets, grants, revocations);

        revocations.Save(new RevocationRecord("grant", "grant-1", "test", 1, "trail:T1"));
        Assert.False(verifier.Verify("grant-1", keyRef));

        using var temp2 = new TempDir();
        var regs2 = new FileCapabilityRegistrationStore(Path.Combine(temp2.Path, "r"));
        var grants2 = new FileGrantStore(Path.Combine(temp2.Path, "g"));
        var revocations2 = new FileRevocationStore(Path.Combine(temp2.Path, "x"));
        regs2.Save(Registration);
        revocations2.Save(new RevocationRecord("registration", "reg-1", "test", 1, "trail:T1"));
        Assert.Throws<InvalidOperationException>(() => new ProductionGrantIssuer(secrets, regs2, grants2, revocations2).Issue(Request, keyRef, "grant-2"));

        using var temp3 = new TempDir();
        var regs3 = new FileCapabilityRegistrationStore(Path.Combine(temp3.Path, "r"));
        var grants3 = new FileGrantStore(Path.Combine(temp3.Path, "g"));
        var revocations3 = new FileRevocationStore(Path.Combine(temp3.Path, "x"));
        regs3.Save(Registration);
        revocations3.Save(new RevocationRecord("key", ProductionGrantIssuer.KeyTarget(keyRef), "test", 1, "trail:T1"));
        Assert.Throws<InvalidOperationException>(() => new ProductionGrantIssuer(secrets, regs3, grants3, revocations3).Issue(Request, keyRef, "grant-3"));
    }

    [Fact]
    public void Missing_key_material_blocks_issuance()
    {
        using var temp = new TempDir();
        var secrets = new MemorySecretProvider();
        var keyRef = new SecretKeyReference("signing", 1, "memory-test", "test");
        var regs = new FileCapabilityRegistrationStore(Path.Combine(temp.Path, "r"));
        regs.Save(Registration);
        var issuer = new ProductionGrantIssuer(
            secrets,
            regs,
            new FileGrantStore(Path.Combine(temp.Path, "g")),
            new FileRevocationStore(Path.Combine(temp.Path, "x")));

        var ex = Assert.Throws<InvalidOperationException>(() => issuer.Issue(Request, keyRef, "grant-1"));
        Assert.Equal("key-material-unavailable", ex.Message);
    }

    [Fact]
    public void Windows_dpapi_provider_refuses_storage_under_canonical_root()
    {
        if (!OperatingSystem.IsWindows()) return;
        Assert.Throws<InvalidOperationException>(() =>
            new WindowsDpapiSecretProvider(
                "D:\\AgentSystem\\global\\.pmcro\\keys",
                "D:\\AgentSystem"));
    }

    [Fact]
    public void Windows_dpapi_round_trip_uses_external_test_storage()
    {
        if (!OperatingSystem.IsWindows()) return;
        var outside = Path.Combine(Path.GetTempPath(), "pmcro-dpapi-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            var provider = new WindowsDpapiSecretProvider(outside, "D:\\AgentSystem");
            var reference = new SecretKeyReference("test-only", 1, "windows-dpapi", "test");
            var value = Encoding.UTF8.GetBytes("non-production-test-material");
            provider.Write(reference, value);
            Assert.True(provider.Exists(reference));
            Assert.Equal(value, provider.Read(reference));
        }
        finally
        {
            if (Directory.Exists(outside)) Directory.Delete(outside, true);
        }
    }

    [Fact]
    public async Task Production_activation_gate_persists_and_revalidates_signed_grant()
    {
        using var temp = new TempDir();
        var secrets = new MemorySecretProvider();
        var keyRef = new SecretKeyReference("activation-key", 1, "memory-test", "test-location");
        secrets.Write(keyRef, Encoding.UTF8.GetBytes("activation-test-key"));
        var regs = new FileCapabilityRegistrationStore(Path.Combine(temp.Path, "r"));
        var grants = new FileGrantStore(Path.Combine(temp.Path, "g"));
        var revocations = new FileRevocationStore(Path.Combine(temp.Path, "x"));
        var activations = new FileActivationStore(Path.Combine(temp.Path, "a"));
        var files = InstalledFiles(temp.Path);
        var manifest = AgentActivationGate.ComputeFileManifestDigest(files);
        var registration = Registration with { FileManifestDigest = manifest };
        regs.Save(registration);
        var grant = new ProductionGrantIssuer(secrets, regs, grants, revocations).Issue(Request, keyRef, "grant-activation");
        var receipt = new AgentActivationReceipt("PASS", "maker", "checker", registration.CandidateDigest, files, grant);
        var ceiling = grant with { DelegatedGrant = ["capability.invoke"], EffectiveGrant = ["capability.invoke"] };

        var gate = new AgentActivationGate(new ProductionGrantVerifier(secrets, grants, revocations), regs, revocations, activations, "memory-test", "test-location");
        var effective = await gate.ActivateAsync("csuite", receipt, ceiling);

        Assert.Equal(["capability.invoke"], effective.EffectiveGrant);
        Assert.True(gate.IsActivated("csuite"));
        Assert.NotNull(activations.Get("csuite"));

        var freshGate = new AgentActivationGate(new ProductionGrantVerifier(secrets, grants, revocations), regs, revocations, new FileActivationStore(Path.Combine(temp.Path, "a")), "memory-test", "test-location");
        Assert.True(freshGate.IsActivated("csuite"));
        File.AppendAllText(files.Keys.Single(), "changed after activation");
        Assert.False(freshGate.IsActivated("csuite"));
        revocations.Save(new RevocationRecord("grant", "grant-activation", "test", 1, "trail:T1"));
        Assert.False(freshGate.IsActivated("csuite"));
    }

    [Fact]
    public async Task Production_activation_gate_fails_closed_on_receipt_and_checker_mismatch()
    {
        using var temp = new TempDir();
        var secrets = new MemorySecretProvider();
        var keyRef = new SecretKeyReference("activation-key", 1, "memory-test", "test-location");
        secrets.Write(keyRef, Encoding.UTF8.GetBytes("activation-test-key"));
        var regs = new FileCapabilityRegistrationStore(Path.Combine(temp.Path, "r"));
        var grants = new FileGrantStore(Path.Combine(temp.Path, "g"));
        var revocations = new FileRevocationStore(Path.Combine(temp.Path, "x"));
        var files = new Dictionary<string, string> { ["candidate.dll"] = new string('d', 64) };
        regs.Save(Registration with { FileManifestDigest = AgentActivationGate.ComputeFileManifestDigest(files) });
        var grant = new ProductionGrantIssuer(secrets, regs, grants, revocations).Issue(Request, keyRef, "grant-activation");
        var gate = new AgentActivationGate(new ProductionGrantVerifier(secrets, grants, revocations), regs, revocations, new FileActivationStore(Path.Combine(temp.Path, "a")), "memory-test", "test-location");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => gate.ActivateAsync("csuite",
            new AgentActivationReceipt("PASS", "checker", "checker", Registration.CandidateDigest, files, grant), grant));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => gate.ActivateAsync("csuite",
            new AgentActivationReceipt("PASS", "maker", "checker", new string('f', 64), files, grant), grant));
        var badFiles = new Dictionary<string, string> { ["candidate.dll"] = new string('e', 64) };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => gate.ActivateAsync("csuite",
            new AgentActivationReceipt("PASS", "maker", "checker", Registration.CandidateDigest, badFiles, grant), grant));
    }

    private static Dictionary<string, string> InstalledFiles(string root)
    {
        var path = System.IO.Path.Combine(root, "candidate.dll");
        System.IO.File.WriteAllText(path, "synthetic candidate bytes");
        return new() { [path] = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.IO.File.ReadAllBytes(path))).ToLowerInvariant() };
    }
    private sealed class MemorySecretProvider : ISecretProvider
    {
        private readonly Dictionary<string, byte[]> _values = new(StringComparer.Ordinal);
        private static string Key(SecretKeyReference r) => r.KeyId + ":" + r.Generation;
        public bool Exists(SecretKeyReference reference) => _values.ContainsKey(Key(reference));
        public byte[] Read(SecretKeyReference reference) => _values[Key(reference)].ToArray();
        public void Write(SecretKeyReference reference, byte[] value) => _values[Key(reference)] = value.ToArray();
    }

    private sealed class TempDir : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "pmcro-control-plane-test-" + Guid.NewGuid().ToString("N"));
        public TempDir() => Directory.CreateDirectory(Path);
        public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, true); }
    }
}
