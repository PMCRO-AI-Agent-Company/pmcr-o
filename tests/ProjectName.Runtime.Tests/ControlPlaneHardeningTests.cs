using System.Security.Cryptography;
using System.Text.Json;
using ProjectName.Contracts;
using ProjectName.Harness;
using ProjectName.Infrastructure;
using Xunit;

namespace ProjectName.Runtime.Tests;

public sealed class ControlPlaneHardeningTests
{
    [Fact]
    public void Operator_policy_rejects_redirected_revocations_and_missing_pins()
    {
        var root = Path.Combine(Path.GetTempPath(), "trust-" + Guid.NewGuid().ToString("N"));
        var paths = new ControlPlaneStatePaths(root, root + "r", root + "g", root + "x", root + "a", root + "k");
        var trust = new ControlPlaneOperatorTrust(paths, "owner");
        trust.ValidatePaths(paths);
        Assert.Throws<UnauthorizedAccessException>(() => trust.ValidatePaths(paths with { RevocationRoot = root + "empty" }));
        var action = new LifecycleActionRequest("r", "bootstrap-first-operational-grant", "t", "s", "c", "c", "missing", "missing", "missing", "missing", "missing", "control-plane.commissioning", "e", "PASS", false);
        Assert.Throws<UnauthorizedAccessException>(() => trust.ValidateBootstrap(action, "other-owner"));
        Assert.Throws<UnauthorizedAccessException>(() => trust.ValidateBootstrap(action, "owner"));
    }

    [Fact]
    public void Immutable_registration_conflict_keeps_original_bytes()
    {
        var root = Path.Combine(Path.GetTempPath(), "immutable-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new FileCapabilityRegistrationStore(root);
            var original = new CapabilityRegistration("r", "c", new string('a', 64), new string('b', 64), "checker", "provenance", "registered");
            store.Save(original);
            store.Save(original);
            Assert.Throws<InvalidOperationException>(() => store.Save(original with { CandidateDigest = new string('c', 64) }));
            Assert.Equal(original, store.Get("r"));
            Assert.Single(Directory.GetFiles(root));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void Missing_and_changed_installed_files_fail_verification()
    {
        var path = Path.Combine(Path.GetTempPath(), "installed-" + Guid.NewGuid().ToString("N"));
        var files = new Dictionary<string, string> { [path] = new string('a', 64) };
        Assert.False(AgentActivationGate.VerifyInstalledFiles(files));
        try
        {
            File.WriteAllText(path, "checked bytes");
            files[path] = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
            Assert.True(AgentActivationGate.VerifyInstalledFiles(files));
            File.AppendAllText(path, "tampered");
            Assert.False(AgentActivationGate.VerifyInstalledFiles(files));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public async Task Production_host_without_trust_cannot_use_test_activation()
    {
        var gate = new DisabledProductionActivation();
        Assert.False(gate.IsActivated("csuite.executive-council"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => gate.ActivateAsync("csuite.executive-council", null!, null!));
    }

    [Fact]
    public void Operator_trust_rejects_request_path_redirection()
    {
        using var temp = new TempDir();
        var trusted = new ControlPlaneStatePaths(
            temp.Path,
            Path.Combine(temp.Path, "registry"),
            Path.Combine(temp.Path, "grants"),
            Path.Combine(temp.Path, "revocations"),
            Path.Combine(temp.Path, "activations"),
            Path.Combine(temp.Path, "keys"));
        var request = trusted with { GrantRoot = Path.Combine(temp.Path, "attacker-grants") };
        var trust = new ControlPlaneOperatorTrust(trusted, "owner");
        Assert.Throws<UnauthorizedAccessException>(() => trust.ValidatePaths(request));
        Assert.False(Directory.Exists(request.GrantRoot));
    }

    [Fact]
    public void Activation_store_atomic_replace_preserves_previous_committed_version()
    {
        using var temp = new TempDir();
        var root = Path.Combine(temp.Path, "activations");
        var store = new FileActivationStore(root);
        var first = new ActivationRecord(
            "csuite", "active", "receipt-1", "grant-1", "reg-1", new string('a', 64),
            ["capability.invoke"], "checker", "prov", "key", 1, DateTimeOffset.UtcNow,
            new Dictionary<string, string> { [Path.Combine(temp.Path, "candidate.dll")] = new string('b', 64) }, "maker");
        var second = first with { ReceiptDigest = "receipt-2", GrantId = "grant-2" };
        store.Save(first);
        store.Save(second);
        var actual = store.Get("csuite")!;
        Assert.Equal("receipt-2", actual.ReceiptDigest);
        Assert.Equal("grant-2", actual.GrantId);
        Assert.Equal(second.CandidateDigest, actual.CandidateDigest);
        var history = Path.Combine(root, "history");
        Assert.True(Directory.Exists(history));
        Assert.Single(Directory.EnumerateFiles(history, "*.json"));
        Assert.DoesNotContain(Directory.EnumerateFiles(root), p => p.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class TempDir : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "pmcro-hardening-" + Guid.NewGuid().ToString("N"));
        public TempDir() => Directory.CreateDirectory(Path);
        public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, true); }
    }
}
