namespace ProjectName.Core;

using System.Text.Json;
using ProjectName.Contracts;

public static class TrailAutomationRouter
{
    public static PmcroMessageEnvelope ToTriggerMessage(TriggerEvent trigger) =>
        new(trigger.EventId, PmcroMessageKinds.TriggerEvent, trigger.TrailId,
            trigger.SourceAgent, trigger.TargetAgent, JsonSerializer.Serialize(trigger), DateTimeOffset.UtcNow,
            ParentTrailId: trigger.ParentTrailId, SeedRef: trigger.SeedRef, EvidenceRefs: trigger.EvidenceRefs,
            CausationId: trigger.CausationId, CorrelationId: trigger.CorrelationId);

    public static PmcroMessageEnvelope ToSeedCandidate(TriggerEvent trigger)
    {
        var seed = new SeedEnvelope(trigger.SeedRef, trigger.TrailId, trigger.TargetAgent,
            trigger.Intent, "queued", null, false, null);
        return new PmcroMessageEnvelope(
            Guid.NewGuid().ToString("D"), PmcroMessageKinds.SeedCandidate, trigger.TrailId,
            trigger.SourceAgent, trigger.TargetAgent, JsonSerializer.Serialize(seed), DateTimeOffset.UtcNow,
            ParentTrailId: trigger.ParentTrailId, SeedRef: trigger.SeedRef, EvidenceRefs: trigger.EvidenceRefs,
            CausationId: trigger.EventId, CorrelationId: trigger.CorrelationId);
    }
}

public static class LifecycleActionGuard
{
    public static void EnsureExecutable(LifecycleActionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!request.RequestsMutation) return;

        Require(request.TrailId, "trail-ref-required");
        Require(request.SeedRef, "seed-ref-required");
        Require(request.CausationId, "causation-id-required");
        Require(request.CorrelationId, "correlation-id-required");
        Require(request.AuthorizationRef, "authorization-ref-required");
        Require(request.CheckerVerdictRef, "checker-verdict-ref-required");
        Require(request.ProvenanceRef, "provenance-ref-required");
        Require(request.AdmissionRef, "admission-ref-required");
        Require(request.GrantVerificationRef, "grant-verification-ref-required");
        Require(request.CapabilityId, "capability-id-required");
        Require(request.SourceEventId, "source-event-id-required");

        if (!StringComparer.Ordinal.Equals(request.CheckerVerdict, "PASS"))
            throw new UnauthorizedAccessException("independent-checker-pass-required");
        if (!request.GrantVerified)
            throw new UnauthorizedAccessException("verified-grant-boundary-required");
    }

    public static ActionReceipt CreateReceipt(
        LifecycleActionRequest request,
        string status,
        IReadOnlyList<string>? evidenceRefs = null)
    {
        EnsureExecutable(request);
        return new ActionReceipt(
            Guid.NewGuid().ToString("D"), request.RequestId, request.TrailId, request.Operation,
            status, request.CausationId, request.CorrelationId, request.AuthorizationRef,
            request.CheckerVerdictRef, request.ProvenanceRef, evidenceRefs ?? Array.Empty<string>(),
            DateTimeOffset.UtcNow);
    }

    private static void Require(string value, string error)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new UnauthorizedAccessException(error);
    }
}
