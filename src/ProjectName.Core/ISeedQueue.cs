namespace ProjectName.Core;

using ProjectName.Contracts;

/// <summary>
/// Queues SeedEnvelopes without interrupting the current Cycle.
/// MESSAGE ARRIVAL ≠ PHASE TRANSITION.
/// </summary>
public interface ISeedQueue
{
    void Enqueue(SeedEnvelope seed);
    SeedEnvelope? Peek();
    SeedEnvelope? DequeueAccepted();
    IReadOnlyList<SeedEnvelope> Snapshot();
}

public sealed class InMemorySeedQueue : ISeedQueue
{
    private readonly Queue<SeedEnvelope> _q = new();
    public void Enqueue(SeedEnvelope seed) => _q.Enqueue(seed with { Status = "queued", Phase = null, GrantsAuthority = false });
    public SeedEnvelope? Peek() => _q.Count > 0 ? _q.Peek() : null;
    public SeedEnvelope? DequeueAccepted() => _q.Count > 0 ? _q.Dequeue() with { Status = "accepted" } : null;
    public IReadOnlyList<SeedEnvelope> Snapshot() => _q.ToArray();
}
