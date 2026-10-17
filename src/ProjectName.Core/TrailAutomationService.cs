namespace ProjectName.Core;

using System.Text.Json;
using ProjectName.Contracts;

public sealed class TrailAutomationService(IMessageBus bus, ISeedController seedController)
{
    public PmcroMessageEnvelope ReceiveTrigger(TriggerEvent trigger)
    {
        ArgumentNullException.ThrowIfNull(trigger);
        Require(trigger.EventId, "event-id-required");
        Require(trigger.TrailId, "trail-id-required");
        Require(trigger.SeedRef, "seed-ref-required");
        Require(trigger.CausationId, "causation-id-required");
        Require(trigger.CorrelationId, "correlation-id-required");

        var triggerMessage = TrailAutomationRouter.ToTriggerMessage(trigger);
        bus.Publish(triggerMessage);

        var seedMessage = TrailAutomationRouter.ToSeedCandidate(trigger);
        bus.Publish(seedMessage);
        return seedMessage;
    }

    public PmcroMessageEnvelope AdmitSeed(PmcroMessageEnvelope seedMessage)
    {
        ArgumentNullException.ThrowIfNull(seedMessage);
        if (!StringComparer.Ordinal.Equals(seedMessage.Kind, PmcroMessageKinds.SeedCandidate))
            throw new ArgumentException("seed-candidate-message-required");

        var seed = JsonSerializer.Deserialize<SeedEnvelope>(seedMessage.PayloadJson)
            ?? throw new InvalidOperationException("seed-payload-invalid");
        var decision = seedController.Accept(new SeedDecision(
            SeedDecisionKind.Accept,
            seed.RawIntent,
            seed.TargetAgent,
            null,
            0,
            null,
            seed.SeedId,
            "trail-automation-admission"));

        var admissionMessage = new PmcroMessageEnvelope(
            Guid.NewGuid().ToString("D"),
            PmcroMessageKinds.SeedAdmission,
            seed.TrailId,
            "orchestrator",
            seed.TargetAgent,
            JsonSerializer.Serialize(decision),
            DateTimeOffset.UtcNow,
            SeedRef: seed.SeedId,
            EvidenceRefs: seedMessage.Evidence,
            CausationId: seedMessage.MessageId,
            CorrelationId: seedMessage.CorrelationId ?? seed.TrailId);

        bus.Publish(admissionMessage);
        return admissionMessage;
    }

    public LifecycleActionRequest CreateActionRequest(
        TriggerEvent trigger,
        PmcroMessageEnvelope admissionMessage,
        string operation,
        string capabilityId,
        string authorizationRef,
        string checkerVerdictRef,
        string provenanceRef,
        string grantVerificationRef,
        string checkerVerdict,
        bool grantVerified)
    {
        if (!StringComparer.Ordinal.Equals(admissionMessage.Kind, PmcroMessageKinds.SeedAdmission))
            throw new UnauthorizedAccessException("admission-message-required");
        if (!StringComparer.Ordinal.Equals(admissionMessage.TrailId, trigger.TrailId))
            throw new UnauthorizedAccessException("trail-admission-mismatch");
        if (!StringComparer.Ordinal.Equals(admissionMessage.SeedRef, trigger.SeedRef))
            throw new UnauthorizedAccessException("seed-admission-mismatch");

        var request = new LifecycleActionRequest(
            Guid.NewGuid().ToString("D"), operation, trigger.TrailId, trigger.SeedRef,
            admissionMessage.MessageId, trigger.CorrelationId, authorizationRef,
            checkerVerdictRef, provenanceRef, admissionMessage.MessageId,
            grantVerificationRef, capabilityId, trigger.EventId, checkerVerdict, grantVerified);

        var requestMessage = new PmcroMessageEnvelope(
            request.RequestId, PmcroMessageKinds.LifecycleActionRequest, request.TrailId,
            "orchestrator", "lifecycle-action", JsonSerializer.Serialize(request), DateTimeOffset.UtcNow,
            SeedRef: request.SeedRef,
            EvidenceRefs: [authorizationRef, checkerVerdictRef, provenanceRef, grantVerificationRef],
            CausationId: request.CausationId,
            CorrelationId: request.CorrelationId);

        bus.Publish(requestMessage);
        return request;
    }

    public PmcroMessageEnvelope RecordReceipt(
        LifecycleActionRequest request,
        string status,
        IReadOnlyList<string>? evidenceRefs = null)
    {
        var receipt = LifecycleActionGuard.CreateReceipt(request, status, evidenceRefs);
        var message = new PmcroMessageEnvelope(
            receipt.ReceiptId, PmcroMessageKinds.ActionReceipt, receipt.TrailId,
            "lifecycle-action", "trail", JsonSerializer.Serialize(receipt), DateTimeOffset.UtcNow,
            SeedRef: request.SeedRef,
            EvidenceRefs: receipt.EvidenceRefs,
            CausationId: request.RequestId,
            CorrelationId: receipt.CorrelationId);
        bus.Publish(message);
        return message;
    }

    public IReadOnlyList<PmcroMessageEnvelope> Trace(string correlationId) =>
        bus.Snapshot()
            .Where(m => StringComparer.Ordinal.Equals(m.CorrelationId, correlationId))
            .OrderBy(m => m.CreatedAt)
            .ToArray();

    private static void Require(string value, string error)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(error);
    }
}
