using System.Text.Json;
using ProjectName.Contracts;
using ProjectName.Core;
using Xunit;

namespace ProjectName.Runtime.Tests;

public sealed class TrailAutomationTests
{
    [Theory]
    [InlineData(TriggerSourceKind.Schedule)]
    [InlineData(TriggerSourceKind.ReflectorNextSeed)]
    [InlineData(TriggerSourceKind.ExecutiveDelegation)]
    [InlineData(TriggerSourceKind.McpEvent)]
    [InlineData(TriggerSourceKind.QueueMessage)]
    public void Automatic_sources_only_create_non_authoritative_trail_messages(TriggerSourceKind source)
    {
        var trigger = Trigger(source);
        var message = TrailAutomationRouter.ToTriggerMessage(trigger);
        var seedMessage = TrailAutomationRouter.ToSeedCandidate(trigger);
        var seed = JsonSerializer.Deserialize<SeedEnvelope>(seedMessage.PayloadJson)!;

        Assert.False(trigger.GrantsAuthority);
        Assert.False(trigger.CanMutate);
        Assert.False(message.GrantsAuthority);
        Assert.False(seedMessage.GrantsAuthority);
        Assert.Equal(PmcroMessageKinds.SeedCandidate, seedMessage.Kind);
        Assert.Equal(trigger.TrailId, seed.TrailId);
        Assert.Equal(trigger.SeedRef, seed.SeedId);
        Assert.Null(seed.Phase);
        Assert.False(seed.GrantsAuthority);
    }

    [Theory]
    [InlineData(LifecycleOperations.ProvisionKeyGeneration)]
    [InlineData(LifecycleOperations.RegisterCapability)]
    [InlineData(LifecycleOperations.IssueMinimumGrant)]
    [InlineData(LifecycleOperations.ActivateCapability)]
    [InlineData(LifecycleOperations.Revoke)]
    [InlineData(LifecycleOperations.ExecuteCapability)]
    public void Mutating_actions_cannot_fire_directly_from_automatic_event(string operation)
    {
        var request = Request(operation) with { AdmissionRef = "", GrantVerificationRef = "", GrantVerified = false };
        Assert.Throws<UnauthorizedAccessException>(() => LifecycleActionGuard.EnsureExecutable(request));
    }

    [Fact]
    public void Mutation_requires_independent_checker_pass_and_verified_grant()
    {
        var checkerFail = Request(LifecycleOperations.RegisterCapability) with { CheckerVerdict = "FAIL" };
        Assert.Throws<UnauthorizedAccessException>(() => LifecycleActionGuard.EnsureExecutable(checkerFail));

        var grantFail = Request(LifecycleOperations.RegisterCapability) with { GrantVerified = false };
        Assert.Throws<UnauthorizedAccessException>(() => LifecycleActionGuard.EnsureExecutable(grantFail));
    }

    [Fact]
    public void Mutating_request_requires_complete_trail_causal_chain()
    {
        var required = new Func<LifecycleActionRequest, LifecycleActionRequest>[]
        {
            r => r with { TrailId = "" },
            r => r with { SeedRef = "" },
            r => r with { CausationId = "" },
            r => r with { CorrelationId = "" },
            r => r with { AuthorizationRef = "" },
            r => r with { CheckerVerdictRef = "" },
            r => r with { ProvenanceRef = "" },
            r => r with { AdmissionRef = "" },
            r => r with { GrantVerificationRef = "" },
        };

        foreach (var remove in required)
            Assert.Throws<UnauthorizedAccessException>(() => LifecycleActionGuard.EnsureExecutable(remove(Request(LifecycleOperations.ActivateCapability))));
    }

    [Fact]
    public void Fully_bound_request_can_produce_non_authoritative_receipt()
    {
        var request = Request(LifecycleOperations.ExecuteCapability);
        LifecycleActionGuard.EnsureExecutable(request);
        var receipt = LifecycleActionGuard.CreateReceipt(request, "executed", ["evidence:checker"]);

        Assert.Equal(request.TrailId, receipt.TrailId);
        Assert.Equal(request.CausationId, receipt.CausationId);
        Assert.Equal(request.CorrelationId, receipt.CorrelationId);
        Assert.False(receipt.GrantsAuthority);
    }

    [Fact]
    public void Inspect_state_is_non_mutating_and_does_not_require_execution_authority()
    {
        var request = Request(LifecycleOperations.InspectState) with
        {
            AuthorizationRef = "",
            CheckerVerdictRef = "",
            ProvenanceRef = "",
            AdmissionRef = "",
            GrantVerificationRef = "",
            CheckerVerdict = "",
            GrantVerified = false
        };

        Assert.False(request.RequestsMutation);
        LifecycleActionGuard.EnsureExecutable(request);
    }

    private static TriggerEvent Trigger(TriggerSourceKind source) => new(
        "event-1", source, "trail-1", "continue verified work", "producer", "orchestrator",
        "seed-1", "cause-1", "corr-1", ["evidence:source"]);

    private static LifecycleActionRequest Request(string operation) => new(
        "request-1", operation, "trail-1", "seed-1", "cause-1", "corr-1",
        "authorization:1", "checker:pass:1", "trail:provenance:1", "admission:1",
        "grant-verification:1", "example.capability", "event-1", "PASS", true);
}
