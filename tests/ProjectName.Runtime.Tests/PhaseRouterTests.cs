using ProjectName.Contracts;
using ProjectName.Core;
using ProjectName.Harness;
using ProjectName.Marketplace;
using ProjectName.Capabilities;
using Xunit;

namespace ProjectName.Runtime.Tests;

public class PhaseRouterTests
{
    private static PhaseRouter Create() =>
        new(new FrameworkActivationStub(), new AutoRunOptions { RequirePriorFrameworkActivation = false });

    [Fact]
    public async Task Make_requires_cycleRef()
    {
        var router = Create();
        var req = new PmcroRequest("make", "do work", "pmcro-maker");
        await Assert.ThrowsAsync<ArgumentException>(() => router.DispatchAsync(req));
    }

    [Fact]
    public async Task Make_with_cycleRef_validates()
    {
        var router = Create();
        var req = new PmcroRequest("make", "do work", "pmcro-maker", CycleRef: "cycle-1");
        var ack = await router.DispatchAsync(req);
        Assert.NotNull(ack);
    }

    [Fact]
    public async Task Subagent_cannot_orchestrate()
    {
        var router = Create();
        var req = new PmcroRequest("orchestrate", "admit", "pmcro-orchestrator",
            ParentToolUseId: "toolu_01");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => router.DispatchAsync(req));
    }

    [Fact]
    public async Task Capability_denied_when_in_Denies()
    {
        var resolver = new McpCapabilityResolver();
        var grant = new AgentExecutionGrant(
            ["mcp.terminal"], ["mcp.terminal"], ["mcp.terminal"], ["mcp.terminal"], "test");
        Assert.False(resolver.IsGranted("mcp.terminal", grant));
    }

    [Fact]
    public async Task Activation_requires_distinct_checker()
    {
        var act = new FrameworkActivationStub();
        var grant = new AgentExecutionGrant([], [], [], [], "t");
        var receipt = new AgentActivationReceipt(
            "PASS", "pmcro-maker", "pmcro-maker", "digest",
            new Dictionary<string, string>(), grant);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            act.ActivateAsync("claude-code", receipt, grant));
    }

    [Fact]
    public async Task Router_calls_real_phase_handler()
    {
        var handler = new RecordingPhaseHandler();
        var router = new PhaseRouter(
            new FrameworkActivationStub(),
            new AutoRunOptions { RequirePriorFrameworkActivation = false },
            handler);
        var req = new PmcroRequest("make", "do work", "pmcro-maker", CycleRef: "cycle-1");

        var ack = Assert.IsType<PhaseDispatchAck>(await router.DispatchAsync(req));

        Assert.True(handler.Called);
        Assert.Equal("handled-by-test", ack.Status);
        Assert.False(ack.GrantsAuthority);
    }

    [Fact]
    public async Task Phase_handler_cannot_create_authority()
    {
        var router = new PhaseRouter(
            new FrameworkActivationStub(),
            new AutoRunOptions { RequirePriorFrameworkActivation = false },
            new AuthorityWideningPhaseHandler());
        var req = new PmcroRequest("make", "do work", "pmcro-maker", CycleRef: "cycle-1");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => router.DispatchAsync(req));
    }

    private sealed class RecordingPhaseHandler : IPhaseHandler
    {
        public bool Called { get; private set; }
        public Task<PhaseHandlerResult> HandleAsync(PmcroRequest request, string frameId, CancellationToken ct = default)
        {
            Called = true;
            return Task.FromResult(new PhaseHandlerResult("handled-by-test", ["evidence:test"]));
        }
    }

    private sealed class AuthorityWideningPhaseHandler : IPhaseHandler
    {
        public Task<PhaseHandlerResult> HandleAsync(PmcroRequest request, string frameId, CancellationToken ct = default) =>
            Task.FromResult(new PhaseHandlerResult("bad", [], GrantsAuthority: true));
    }
}
