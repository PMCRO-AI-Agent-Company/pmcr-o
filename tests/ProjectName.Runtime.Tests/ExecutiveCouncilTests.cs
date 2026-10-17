using ProjectName.Contracts;
using ProjectName.Core;
using ProjectName.Infrastructure;
using ProjectName.Runtime.Maf;
using Xunit;

namespace ProjectName.Runtime.Tests;

public sealed class ExecutiveCouncilTests
{
    [Fact]
    public void File_bus_persists_and_claims_without_authority()
    {
        using var temp = new TempDir();
        var first = new FilePmcroMessageBus(temp.Path);
        first.Publish(new PmcroMessageEnvelope(
            "m1", PmcroMessageKinds.ExecutiveRequest, "trail-1", "orchestrator", "cto",
            "{}", DateTimeOffset.UtcNow));

        var second = new FilePmcroMessageBus(temp.Path);
        var claimed = second.Claim("fresh-session", TimeSpan.FromMinutes(1));

        Assert.NotNull(claimed);
        Assert.Equal("fresh-session", claimed!.LeaseOwner);
        Assert.Equal("claimed", claimed.Status);
        Assert.False(claimed.GrantsAuthority);
    }

    [Fact]
    public void Reflector_next_seed_can_be_resumed_by_a_fresh_session()
    {
        using var temp = new TempDir();
        var producer = new MessageBusSeedQueue(new FilePmcroMessageBus(temp.Path), "session-a");
        producer.EnqueueNextSeed("trail-9", new ProjectName.Contracts.NextSeedIntent(
            "continue from verified evidence", "cycle-8", "frame-r", "orchestrator", "queued continuation"));

        var consumer = new MessageBusSeedQueue(new FilePmcroMessageBus(temp.Path), "session-b");
        var accepted = consumer.DequeueAccepted();

        Assert.NotNull(accepted);
        Assert.Equal("trail-9", accepted!.TrailId);
        Assert.Equal("accepted", accepted.Status);
        Assert.Null(accepted.Phase);
        Assert.False(accepted.GrantsAuthority);
    }

    [Fact]
    public void Executive_bundle_preserves_dissent_and_never_becomes_authority()
    {
        var cto = Position("cto", "Use managed containers", 0.82);
        var cfo = Position("cfo", "Delay migration until unit economics improve", 0.77);
        var bundle = new ExecutiveRecommendationBundle(
            "b1", "r1", [cto, cfo], ["Current platform works"],
            ["CTO favors migration while CFO favors delay"], ["validated cost forecast"],
            ["speed versus cost"], null, ["managed container pilot", "delay"],
            ["platform-team"], [], ["validate forecast"], false);

        Assert.False(bundle.Authoritative);
        Assert.False(bundle.GrantsAuthority);
        Assert.Single(bundle.MaterialDisagreements);
        Assert.All(bundle.Positions, p => Assert.False(p.GrantsAuthority));
    }

    [Fact]
    public void Council_escalation_is_bounded_and_non_authoritative()
    {
        var request = new ExecutiveCouncilRequest("r1", "trail", "question", [], [], MaxDebateRounds: 1);
        var debate = Bundle(disagreements: ["material conflict"]);
        Assert.Equal(ExecutiveCouncilNextMode.BoundedGroupChat,
            MafExecutiveCouncilCompiler.SelectNextMode(request, debate));

        var approval = Bundle(requiresOwnerApproval: true);
        Assert.Equal(ExecutiveCouncilNextMode.HumanInTheLoop,
            MafExecutiveCouncilCompiler.SelectNextMode(request, approval));

        var handoff = Bundle(delegations: ["security-review"]);
        Assert.Equal(ExecutiveCouncilNextMode.Handoff,
            MafExecutiveCouncilCompiler.SelectNextMode(request, handoff));
    }

    [Fact]
    public void Executive_visible_identity_uses_full_office_name_and_dependency_protocol()
    {
        var office = new ExecutiveOfficeDefinition(
            "cto", ExecutiveOfficeNames.ChiefTechnologyOfficer,
            "technical strategy and architecture", "Evaluate the technical tradeoff.", ["technology"]);

        var instructions = MafExecutiveCouncilCompiler.BuildOfficeInstructions(office);

        Assert.Equal("I AM the Chief Technology Officer Agent.", office.VisibleIdentity);
        Assert.StartsWith("I AM the Chief Technology Officer Agent.", instructions, StringComparison.Ordinal);
        Assert.Contains("DEPENDS ON:", instructions, StringComparison.Ordinal);
        Assert.Contains("RELATION: ACCEPTS, CHALLENGES, EXTENDS, REQUESTS, or VERIFIES", instructions, StringComparison.Ordinal);
        Assert.Contains("NEXT: <full role name>", instructions, StringComparison.Ordinal);
        Assert.DoesNotContain("I AM the CTO Agent.", instructions, StringComparison.Ordinal);
    }

    [Fact]
    public void Startup_organization_manifest_resolves_company_shape_without_authority()
    {
        var manifest = ExecutiveOrganizationManifestLoader.Parse("""
        {
          "organizationId":"startup",
          "displayName":"Startup Company",
          "shape":"startup",
          "offices":[
            {"officeId":"ceo","displayName":"Chief Executive Officer","scope":"strategy","instructions":"integrate","domainTags":["strategy"]},
            {"officeId":"cto","displayName":"Chief Technology Officer","scope":"technology","instructions":"evaluate","domainTags":["technology"]},
            {"officeId":"cpo","displayName":"Chief Product Officer","scope":"product","instructions":"evaluate","domainTags":["product"]}
          ],
          "defaultOfficeIds":["ceo","cto","cpo"],
          "ownerApprovalOfficeIds":["ceo"]
        }
        """);

        Assert.Equal("startup", manifest.Shape);
        Assert.False(manifest.GrantsAuthority);
        Assert.Equal(["ceo", "cto", "cpo"], manifest.ResolveOffices().Select(o => o.OfficeId).ToArray());
    }

    [Fact]
    public void Bank_organization_manifest_supports_risk_compliance_and_data_offices()
    {
        var manifest = ExecutiveOrganizationManifestLoader.Parse("""
        {
          "organizationId":"bank",
          "displayName":"Banking Organization",
          "shape":"bank",
          "offices":[
            {"officeId":"ceo","displayName":"Chief Executive Officer","scope":"strategy","instructions":"integrate","domainTags":[]},
            {"officeId":"cro","displayName":"Chief Risk Officer","scope":"risk","instructions":"evaluate","domainTags":["risk"]},
            {"officeId":"cco","displayName":"Chief Compliance Officer","scope":"compliance","instructions":"evaluate","domainTags":["compliance"]},
            {"officeId":"cdo","displayName":"Chief Data Officer","scope":"data","instructions":"evaluate","domainTags":["data"]}
          ],
          "defaultOfficeIds":["ceo","cro","cco","cdo"],
          "ownerApprovalOfficeIds":["ceo","cro","cco"]
        }
        """);

        Assert.Equal(["ceo", "cro", "cco", "cdo"], manifest.ResolveOffices().Select(o => o.OfficeId).ToArray());
        Assert.Equal(ExecutiveOfficeNames.ChiefRiskOfficer, manifest.Offices.Single(o => o.OfficeId == "cro").DisplayName);
        Assert.False(manifest.GrantsAuthority);
    }

    [Fact]
    public void Organization_manifest_rejects_unknown_default_office()
    {
        var json = """
        {
          "organizationId":"bad",
          "displayName":"Bad Company",
          "shape":"bad",
          "offices":[{"officeId":"ceo","displayName":"Chief Executive Officer","scope":"strategy","instructions":"integrate","domainTags":[]}],
          "defaultOfficeIds":["missing"],
          "ownerApprovalOfficeIds":[]
        }
        """;

        var ex = Assert.Throws<InvalidOperationException>(() => ExecutiveOrganizationManifestLoader.Parse(json));
        Assert.Equal("unknown-default-executive-office:missing", ex.Message);
    }

    private static ExecutivePosition Position(string office, string position, double confidence) =>
        new(Guid.NewGuid().ToString("D"), "r1", office, position, "bounded rationale",
            [], [], [], [], [], [], [], [], [], confidence);

    private static ExecutiveRecommendationBundle Bundle(
        IReadOnlyList<string>? disagreements = null,
        IReadOnlyList<string>? delegations = null,
        bool requiresOwnerApproval = false) =>
        new("b", "r1", [], [], disagreements ?? [], [], [], null, [], delegations ?? [], [], [], requiresOwnerApproval);

    private sealed class TempDir : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "pmcro-msg-" + Guid.NewGuid().ToString("N"));
        public TempDir() => Directory.CreateDirectory(Path);
        public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, true); }
    }
}
