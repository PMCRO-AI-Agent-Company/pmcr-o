using ProjectName.Contracts;
using Xunit;

namespace ProjectName.Runtime.Tests;

public class AllFramesTests
{
    [Fact]
    public void All_25_routes_match_catalog()
    {
        var roles = PmcroConstants.Roles;
        var phases = PmcroConstants.Phases;
        var expected = new List<string>();
        foreach (var r in roles)
        foreach (var p in phases)
            expected.Add(PmcroFrameCatalog.Route(r, p));
        Assert.Equal(25, expected.Count);
        Assert.Equal("@orchestrator /pmcro:orchestrate", expected[0]);
        Assert.Equal("@reflector /pmcro:reflect", expected[24]);
        for (var i = 0; i < 25; i++)
            Assert.Equal(expected[i],
                PmcroFrameCatalog.Route(
                    PmcroFrameCatalog.AllOrdered[i].Role,
                    PmcroFrameCatalog.AllOrdered[i].Phase));
    }

    [Fact]
    public void Exactly_five_of_twenty_five_turns_are_authoritative()
    {
        var authoritative = PmcroFrameCatalog.AllOrdered.Where(f => f.IsAuthoritativeTurn).ToArray();
        Assert.Equal(5, authoritative.Length);
        Assert.Contains(authoritative, f => f.FrameId == "orchestrator:ORCHESTRATE");
        Assert.Contains(authoritative, f => f.FrameId == "planner:PLAN");
        Assert.Contains(authoritative, f => f.FrameId == "maker:MAKE");
        Assert.Contains(authoritative, f => f.FrameId == "checker:CHECK");
        Assert.Contains(authoritative, f => f.FrameId == "reflector:REFLECT");
        Assert.Equal(20, PmcroFrameCatalog.AllOrdered.Count(f => !f.IsAuthoritativeTurn));
    }
}
