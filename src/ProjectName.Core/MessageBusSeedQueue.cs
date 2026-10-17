namespace ProjectName.Core;

using System.Text.Json;
using ProjectName.Contracts;

public sealed class MessageBusSeedQueue(IMessageBus bus, string workerId = "pmcro-session") : ISeedQueue
{
    private static readonly IReadOnlySet<string> SeedKinds = new HashSet<string>(StringComparer.Ordinal)
    {
        PmcroMessageKinds.SeedCandidate,
        PmcroMessageKinds.NextSeed,
        PmcroMessageKinds.ResumeTrail,
    };

    public void Enqueue(SeedEnvelope seed)
    {
        var queued = seed with { Status = "queued", Phase = null, GrantsAuthority = false };
        bus.Publish(new PmcroMessageEnvelope(
            Guid.NewGuid().ToString("D"), PmcroMessageKinds.SeedCandidate, queued.TrailId,
            "seed-queue", queued.TargetAgent, JsonSerializer.Serialize(queued), DateTimeOffset.UtcNow,
            SeedRef: queued.SeedId, CorrelationId: queued.TrailId));
    }

    public void EnqueueNextSeed(string trailId, ProjectName.Contracts.NextSeedIntent nextSeed, string sourceAgent = "reflector", string? sourceSessionId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trailId);
        var seed = new SeedEnvelope(Guid.NewGuid().ToString("D"), trailId, nextSeed.SuggestedRole,
            nextSeed.Intent, "queued", null, false, null);
        bus.Publish(new PmcroMessageEnvelope(
            Guid.NewGuid().ToString("D"), PmcroMessageKinds.NextSeed, trailId, sourceAgent,
            nextSeed.SuggestedRole, JsonSerializer.Serialize(seed), DateTimeOffset.UtcNow,
            SourceSessionId: sourceSessionId, SeedRef: seed.SeedId, CycleRef: nextSeed.SourceCycleRef,
            FrameRef: nextSeed.SourceFrameId, CorrelationId: trailId));
    }

    public SeedEnvelope? Peek() => Deserialize(bus.Peek(SeedKinds));

    public SeedEnvelope? DequeueAccepted()
    {
        var claimed = bus.Claim(workerId, TimeSpan.FromMinutes(5), SeedKinds);
        var seed = Deserialize(claimed);
        return seed is null ? null : seed with { Status = "accepted", Phase = null, GrantsAuthority = false };
    }

    public IReadOnlyList<SeedEnvelope> Snapshot() => bus.Snapshot()
        .Where(m => SeedKinds.Contains(m.Kind))
        .Select(Deserialize)
        .OfType<SeedEnvelope>()
        .ToArray();

    private static SeedEnvelope? Deserialize(PmcroMessageEnvelope? message) =>
        message is null ? null : JsonSerializer.Deserialize<SeedEnvelope>(message.PayloadJson);
}
