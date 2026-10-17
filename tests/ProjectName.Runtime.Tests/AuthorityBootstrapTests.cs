using ProjectName.Contracts;
using ProjectName.Core;
using Xunit;

namespace ProjectName.Runtime.Tests;

public class AuthorityBootstrapTests
{
    private static readonly string CandidateDigest = new('a', 64);
    private static readonly string ManifestDigest = new('b', 64);

    private static CapabilityRegistration Registration(string status = "registered", string capability = "example.install", string digest = null!) =>
        new("reg-1", capability, digest ?? CandidateDigest, ManifestDigest, "checker:pass", "trail:T1", status);

    private static GrantDerivationRequest Request(
        IReadOnlyList<string>? requested = null,
        IReadOnlyList<string>? parent = null,
        IReadOnlyList<string>? host = null,
        IReadOnlyList<string>? environment = null,
        IReadOnlyList<string>? denies = null,
        string capability = "example.install",
        string digest = null!) =>
        new("maker", "reg-1", capability, digest ?? CandidateDigest,
            requested ?? ["capability.invoke", "example.install"],
            parent ?? ["capability.invoke", "example.install"],
            host ?? ["capability.invoke", "example.install"],
            environment ?? ["capability.invoke", "example.install"],
            denies ?? [], "checker:pass", "trail:T1");

    [Fact]
    public void Registration_must_be_registered_and_evidence_bound()
    {
        var service = new CapabilityRegistrationService();
        Assert.True(service.Validate(Registration()));
        Assert.False(service.Validate(Registration(status: "revoked")));
        Assert.False(service.Validate(Registration(digest: "bad")));
    }

    [Fact]
    public void Effective_grant_is_monotonic_intersection()
    {
        var service = new GrantDerivationService();
        var request = Request(
            requested: ["capability.invoke", "example.install", "requested-only"],
            parent: ["capability.invoke", "example.install", "parent-only"],
            host: ["capability.invoke", "example.install", "host-only"],
            environment: ["capability.invoke", "example.install", "env-only"]);
        var grant = service.Derive(request, Registration(), "test-key", "g-1");
        Assert.Equal(["capability.invoke", "example.install"], grant.EffectiveGrant);
        Assert.Equal(grant.EffectiveGrant, grant.DelegatedGrant);
    }

    [Fact]
    public void Denies_always_win()
    {
        var service = new GrantDerivationService();
        var grant = service.Derive(Request(denies: ["example.install"]), Registration(), "test-key", "g-1");
        Assert.Contains("capability.invoke", grant.EffectiveGrant!);
        Assert.DoesNotContain("example.install", grant.EffectiveGrant!);
    }

    [Fact]
    public void Parent_host_and_environment_overflow_are_removed()
    {
        var service = new GrantDerivationService();
        var grant = service.Derive(
            Request(requested: ["capability.invoke", "example.install", "outside"],
                parent: ["capability.invoke", "example.install"],
                host: ["capability.invoke", "example.install"],
                environment: ["capability.invoke", "example.install"]),
            Registration(), "test-key", "g-1");
        Assert.DoesNotContain("outside", grant.EffectiveGrant!);
    }

    [Fact]
    public void Unregistered_candidate_cannot_receive_core_grant()
    {
        var service = new GrantDerivationService();
        Assert.Throws<InvalidOperationException>(() =>
            service.Derive(Request(), Registration(status: "revoked"), "test-key", "g-1"));
    }

    [Fact]
    public void Candidate_digest_and_capability_bindings_are_exact()
    {
        var service = new GrantDerivationService();
        Assert.Throws<InvalidOperationException>(() =>
            service.Derive(Request(digest: new string('c', 64)), Registration(), "test-key", "g-1"));
        Assert.Throws<InvalidOperationException>(() =>
            service.Derive(Request(capability: "other.capability"), Registration(), "test-key", "g-1"));
    }

    [Fact]
    public void Forged_or_tampered_core_grants_fail_proof()
    {
        var service = new GrantDerivationService();
        var grant = service.Derive(Request(), Registration(), "test-key", "g-1");
        Assert.True(service.VerifyProof(grant, "test-key"));
        Assert.False(service.VerifyProof(grant with { IssuerProof = "00" }, "test-key"));
        Assert.False(service.VerifyProof(grant with { Issuer = "marketplace" }, "test-key"));
        Assert.False(service.VerifyProof(grant with { EffectiveGrant = ["capability.invoke", "example.install", "outside"] }, "test-key"));
        Assert.False(service.VerifyProof(grant with { CapabilityId = "other.capability" }, "test-key"));
    }

    [Fact]
    public void Missing_core_signing_key_is_rejected()
    {
        var service = new GrantDerivationService();
        Assert.Throws<InvalidOperationException>(() =>
            service.Derive(Request(), Registration(), "", "g-1"));
    }

    [Fact]
    public void Bootstrap_path_escape_is_not_an_exact_allowlist_match()
    {
        var allowed = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "root", "allowed.cs"));
        var escaped = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(allowed)!, "..", "outside.cs"));
        Assert.False(StringComparer.OrdinalIgnoreCase.Equals(allowed, escaped));
    }
}
