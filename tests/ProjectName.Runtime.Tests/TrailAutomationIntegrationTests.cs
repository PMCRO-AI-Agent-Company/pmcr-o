using Microsoft.Extensions.DependencyInjection;
using ProjectName.Contracts;
using ProjectName.Core;
using ProjectName.Harness;
using ProjectName.Infrastructure;
using Xunit;

namespace ProjectName.Runtime.Tests;

public sealed class TrailAutomationIntegrationTests
{
    [Fact]
    public void Trigger_is_saved_seeded_and_admitted_through_normal_controller()
    {
        using var temp = new TempDir();
        var bus = new FilePmcroMessageBus(temp.Path);
        var service = new TrailAutomationService(bus, new SeedController());
        var trigger = Trigger("trail-child", "trail-parent", "corr-1");

        var seed = service.ReceiveTrigger(trigger);
        var admission = service.AdmitSeed(seed);
        var messages = bus.Snapshot();

        Assert.Contains(messages, m => m.Kind == PmcroMessageKinds.TriggerEvent);
        Assert.Contains(messages, m => m.Kind == PmcroMessageKinds.SeedCandidate);
        Assert.Contains(messages, m => m.Kind == PmcroMessageKinds.SeedAdmission);
        Assert.Equal(trigger.TrailId, admission.TrailId);
        Assert.Equal(trigger.SeedRef, admission.SeedRef);
        Assert.False(admission.GrantsAuthority);
    }

    [Fact]
    public void Action_request_can_be_recorded_but_cannot_be_receipted_without_permission()
    {
        using var temp = new TempDir();
        var bus = new FilePmcroMessageBus(temp.Path);
        var service = new TrailAutomationService(bus, new SeedController());
        var trigger = Trigger("trail-1", null, "corr-2");
        var admission = service.AdmitSeed(service.ReceiveTrigger(trigger));

        var request = service.CreateActionRequest(trigger, admission,
            LifecycleOperations.RegisterCapability, "example.capability",
            "authorization:1", "checker:1", "trail:1", "grant:1", "FAIL", false);

        Assert.Contains(bus.Snapshot(), m => m.Kind == PmcroMessageKinds.LifecycleActionRequest);
        Assert.Throws<UnauthorizedAccessException>(() => service.RecordReceipt(request, "executed"));
        Assert.DoesNotContain(bus.Snapshot(), m => m.Kind == PmcroMessageKinds.ActionReceipt);
    }

    [Fact]
    public void Validated_action_receipt_returns_to_same_trail_and_correlation_chain()
    {
        using var temp = new TempDir();
        var bus = new FilePmcroMessageBus(temp.Path);
        var service = new TrailAutomationService(bus, new SeedController());
        var trigger = Trigger("trail-1", null, "corr-3");
        var admission = service.AdmitSeed(service.ReceiveTrigger(trigger));
        var request = service.CreateActionRequest(trigger, admission,
            LifecycleOperations.ExecuteCapability, "example.capability",
            "authorization:1", "checker:pass:1", "trail:1", "grant:verified:1", "PASS", true);

        var receipt = service.RecordReceipt(request, "executed", ["evidence:result"]);
        var trace = service.Trace("corr-3");

        Assert.Equal("trail-1", receipt.TrailId);
        Assert.Equal(request.RequestId, receipt.CausationId);
        Assert.Equal("corr-3", receipt.CorrelationId);
        Assert.Equal(5, trace.Count);
        Assert.Equal(PmcroMessageKinds.TriggerEvent, trace[0].Kind);
        Assert.Equal(PmcroMessageKinds.ActionReceipt, trace[^1].Kind);
        Assert.All(trace, message => Assert.False(message.GrantsAuthority));
    }

    [Fact]
    public void Parent_and_child_trails_can_share_one_correlation_chain()
    {
        using var temp = new TempDir();
        var bus = new FilePmcroMessageBus(temp.Path);
        var service = new TrailAutomationService(bus, new SeedController());

        service.ReceiveTrigger(Trigger("trail-parent", null, "company-work"));
        service.ReceiveTrigger(Trigger("trail-child", "trail-parent", "company-work"));
        var trace = service.Trace("company-work");

        Assert.Contains(trace, m => m.TrailId == "trail-parent");
        Assert.Contains(trace, m => m.TrailId == "trail-child" && m.ParentTrailId == "trail-parent");
    }

    [Fact]
    public void Durable_runtime_registers_trail_automation_service()
    {
        using var temp = new TempDir();
        var services = new ServiceCollection();
        services.AddProjectNameAgentSystem(messageBusRoot: temp.Path);
        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<TrailAutomationService>());
        Assert.NotNull(provider.GetService<IMessageBus>());
    }

    private static TriggerEvent Trigger(string trail, string? parent, string correlation) => new(
        Guid.NewGuid().ToString("D"), TriggerSourceKind.Schedule, trail, "continue safe work",
        "schedule", "orchestrator", Guid.NewGuid().ToString("D"), Guid.NewGuid().ToString("D"),
        correlation, ["evidence:trigger"], parent);

    private sealed class TempDir : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "pmcro-auto-int-" + Guid.NewGuid().ToString("N"));
        public TempDir() => Directory.CreateDirectory(Path);
        public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, true); }
    }
}
