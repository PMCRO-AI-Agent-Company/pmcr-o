using System.Security.Cryptography;
using System.Text.Json;
using ProjectName.Contracts;
using ProjectName.Core;
using ProjectName.Harness;
using ProjectName.Infrastructure;
using Xunit;

namespace ProjectName.Runtime.Tests;

public sealed class FirstOperationalGrantBootstrapTests
{
    [Fact]
    public void Bootstrap_is_scoped_single_use_and_verifies_with_fresh_dpapi_provider()
    {
        if (!OperatingSystem.IsWindows()) return;
        using var f = new Fixture();
        Assert.True(f.Service.Run(f.Request, "preflight").Ok);
        Assert.False(Directory.Exists(f.Request.Paths.ProtectedKeyRoot));
        Assert.True(f.Service.Run(f.Request, "execute").Ok);
        Assert.Throws<UnauthorizedAccessException>(() => f.Service.Run(f.Request, "execute"));
        var fresh = new ProductionGrantVerifier(new WindowsDpapiSecretProvider(f.Request.Paths.ProtectedKeyRoot, f.Root),
            new FileGrantStore(f.Request.Paths.GrantRoot), new FileRevocationStore(f.Request.Paths.RevocationRoot));
        var grant = f.Grants.Get(FirstOperationalGrantBootstrap.GrantId)!;
        Assert.True(fresh.Verify(grant, f.Key));
        Assert.Equal(FirstOperationalGrantBootstrap.Permissions.Order(), grant.EffectiveGrant!.Order());
        Assert.DoesNotContain(grant.EffectiveGrant!, p => p.Contains("unrelated") || p.Contains("provision-key"));
        Assert.True(f.Service.Run(f.Request, "check").Ok);
    }

    [Theory]
    [InlineData("owner")]
    [InlineData("expired")]
    [InlineData("bytes")]
    [InlineData("stale-checker")]
    [InlineData("target")]
    [InlineData("preexisting-key")]
    [InlineData("partial")]
    public void Invalid_bootstrap_fails_without_issuing_grant(string defect)
    {
        if (!OperatingSystem.IsWindows()) return;
        using var f = new Fixture();
        var request = f.Request;
        switch (defect)
        {
            case "owner": request = request with { OwnerPrincipal = "other-owner" }; break;
            case "expired": f.WriteAuthorization(DateTimeOffset.UtcNow.AddMinutes(-1)); break;
            case "bytes": File.AppendAllText(f.Candidate, "changed"); break;
            case "stale-checker": f.WriteChecker(DateTimeOffset.UtcNow.AddDays(-2)); break;
            case "target": request = request with { TrailAction = request.TrailAction with { CapabilityId = "unrelated" } }; break;
            case "preexisting-key": f.Secrets.Write(f.Key, RandomNumberGenerator.GetBytes(32)); break;
            case "partial":
                var state = Path.Combine(f.Root, "global", ".pmcro", "bootstrap", "first-operational-grant");
                Directory.CreateDirectory(state);
                File.WriteAllText(Path.Combine(state, "claimed.json"), "{}");
                break;
        }
        Assert.Throws<UnauthorizedAccessException>(() => f.Service.Run(request, "execute"));
        Assert.Null(f.Grants.Get(FirstOperationalGrantBootstrap.GrantId));
    }

    private sealed class Fixture : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "bootstrap-test-" + Guid.NewGuid().ToString("N"));
        public string Candidate => Path.Combine(Root, "candidate.txt");
        public FirstOperationalGrantRequest Request { get; }
        public FirstOperationalGrantBootstrap Service { get; }
        public FileGrantStore Grants { get; }
        public WindowsDpapiSecretProvider Secrets { get; }
        public SecretKeyReference Key => new(FirstOperationalGrantBootstrap.KeyId, 1, "windows-dpapi", Request.Paths.ProtectedKeyRoot);
        private readonly Dictionary<string, string> files;
        public Fixture()
        {
            Directory.CreateDirectory(Root);
            File.WriteAllText(Candidate, "synthetic candidate, not production authority");
            files = new() { [Candidate] = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Candidate))).ToLowerInvariant() };
            var state = Path.Combine(Root, "global", ".pmcro");
            var paths = new ControlPlaneStatePaths(Root, Path.Combine(state, "registry", "records"), Path.Combine(state, "grants", "records"),
                Path.Combine(state, "revocations"), Path.Combine(state, "activations"), Root + "-keys");
            var action = new LifecycleActionRequest("test", FirstOperationalGrantBootstrap.Operation, "trail", "seed", "cause", "corr",
                Path.Combine(Root, "owner.json"), Path.Combine(Root, "checker.json"), Path.Combine(Root, "provenance.json"),
                Path.Combine(Root, "admission.json"), Path.Combine(Root, "owner.json"), "control-plane.commissioning", "event", "PASS", false);
            Request = new(action, paths, "reg-test", "synthetic-owner");
            WriteAuthorization(DateTimeOffset.UtcNow.AddHours(1));
            WriteChecker(DateTimeOffset.UtcNow.AddSeconds(-1));
            File.WriteAllText(action.ProvenanceRef, "{\"trailId\":\"trail\"}");
            File.WriteAllText(action.AdmissionRef, "{\"admitted\":true}");
            var regs = new FileCapabilityRegistrationStore(paths.RegistrationRoot);
            regs.Save(new("reg-test", "control-plane.commissioning", new string('a', 64), AgentActivationGate.ComputeFileManifestDigest(files), action.CheckerVerdictRef, action.ProvenanceRef, "registered"));
            Secrets = new(paths.ProtectedKeyRoot, Root);
            Grants = new(paths.GrantRoot);
            var revocations = new FileRevocationStore(paths.RevocationRoot);
            Service = new(Secrets, regs, Grants, new ProductionGrantIssuer(Secrets, regs, Grants, revocations),
                new ProductionGrantVerifier(Secrets, Grants, revocations), "synthetic-owner",
                new ControlPlaneOperatorTrust(paths, "synthetic-owner", Hash(action.AuthorizationRef), Hash(action.CheckerVerdictRef)));
        }
        public void WriteAuthorization(DateTimeOffset expiresAt) => File.WriteAllText(Request.TrailAction.AuthorizationRef, JsonSerializer.Serialize(new
        {
            status = "open", operation = FirstOperationalGrantBootstrap.Operation, ownerPrincipal = "synthetic-owner",
            trailId = "trail", seedRef = "seed", registrationId = "reg-test", candidateDigest = new string('a', 64), expiresAt
        }));
        public void WriteChecker(DateTimeOffset checkedAt) => File.WriteAllText(Request.TrailAction.CheckerVerdictRef, JsonSerializer.Serialize(new
        {
            verdict = "PASS", independent = true, makerIdentity = "maker", checkerIdentity = "checker", candidateDigest = new string('a', 64), files, checkedAt
        }));
        public void Dispose()
        {
            if (Directory.Exists(Root)) Directory.Delete(Root, true);
            if (Directory.Exists(Root + "-keys")) Directory.Delete(Root + "-keys", true);
        }
        private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    }
}
