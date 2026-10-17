using ProjectName.Contracts;
using ProjectName.Core;
using ProjectName.Harness;
using ProjectName.Marketplace;
using ProjectName.Capabilities;
using Xunit;

namespace ProjectName.Runtime.Tests;

public class ResourceValidatorTests
{
    private static readonly HashSet<string> Catalog = new(StringComparer.Ordinal)
    {
        "create-skill", "add-project", "mcp.terminal"
    };

    [Fact]
    public void Unknown_id_is_gap_with_create_skill_hint()
    {
        var v = new MarketplaceResourceValidator();
        var r = v.Validate(["playwright-magic"], null, Catalog);
        Assert.False(r.Ok);
        Assert.Single(r.Gaps);
        Assert.Equal("create-skill", r.Gaps[0].CreateSkillHint);
    }

    [Fact]
    public void Catalog_without_grant_is_gap_when_grant_required()
    {
        var v = new MarketplaceResourceValidator();
        var grant = new AgentExecutionGrant([], [], [], [], "t");
        var r = v.Validate(["mcp.terminal"], grant, Catalog);
        Assert.False(r.Ok);
        Assert.Contains(r.Gaps, g => g.Reason.Contains("not granted"));
    }

    [Fact]
    public void Granted_catalog_id_validates()
    {
        var v = new MarketplaceResourceValidator();
        var grant = new AgentExecutionGrant(
            ["mcp.terminal"], ["mcp.terminal"], ["mcp.terminal"], [], "t");
        var r = v.Validate(["mcp.terminal"], grant, Catalog);
        Assert.True(r.Ok);
        Assert.Contains("mcp.terminal", r.ValidatedIds);
    }

    [Fact]
    public void Gaps_route_to_create_skill_seed()
    {
        var gaps = new[] { new GapRecord("x", "missing", "author", "create-skill") };
        var seed = CreateSkillRoute.FromGaps(gaps, "c3", "planner:PLAN");
        Assert.Contains("create-skill", seed.Intent);
        Assert.Equal("orchestrator", seed.SuggestedRole);
        Assert.Contains("Do not auto-execute", seed.Notes);
    }
}
