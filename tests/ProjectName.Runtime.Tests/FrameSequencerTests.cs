using ProjectName.Contracts;
using ProjectName.Core;
using ProjectName.Harness;
using ProjectName.Marketplace;
using ProjectName.Capabilities;
using Xunit;

namespace ProjectName.Runtime.Tests;

public class FrameSequencerTests
{
    [Fact]
    public void Catalog_has_25_frames()
    {
        Assert.Equal(25, PmcroFrameCatalog.AllOrdered.Count);
        Assert.Equal("orchestrator:ORCHESTRATE", PmcroFrameCatalog.AllOrdered[0].FrameId);
        Assert.Equal("reflector:REFLECT", PmcroFrameCatalog.AllOrdered[24].FrameId);
    }

    [Fact]
    public void Advance_is_linear_no_jump()
    {
        var seq = new FrameSequencer(0);
        Assert.Equal(0, seq.Current.Ordinal);
        Assert.True(seq.TryAdvance(out var n));
        Assert.Equal(1, n.Ordinal);
        Assert.Equal("orchestrator:PLAN", n.FrameId);
    }

    [Fact]
    public void Halt_stops_advance()
    {
        var seq = new FrameSequencer(0);
        seq.Halt("check failed");
        Assert.False(seq.TryAdvance(out _));
        Assert.Equal("check failed", seq.HaltReason);
    }

    [Fact]
    public async Task Check_fail_emits_next_seed_not_execute()
    {
        var path = new CycleErrorPath();
        var req = new PmcroRequest("check", "verify", "pmcro-checker", CycleRef: "c1", Role: "checker", Phase: "CHECK");
        var seed = await path.OnCheckFailedAsync(req, "path-alias defect");
        Assert.Equal("orchestrator", seed.SuggestedRole);
        Assert.Contains("Repair", seed.Intent);
        Assert.Contains("Do not auto-execute", seed.Notes);
    }
}
