namespace ProjectName.Core;

using ProjectName.Contracts;

/// <summary>
/// Walks all 25 frames in canonical order unless halted.
/// Jumping is forbidden: only TryAdvance or Halt.
/// </summary>
public sealed class FrameSequencer : IFrameSequencer
{
    private int _index;
    public bool IsHalted { get; private set; }
    public string? HaltReason { get; private set; }

    public FrameSequencer(int startOrdinal = 0)
    {
        if (startOrdinal < 0 || startOrdinal > 24)
            throw new ArgumentOutOfRangeException(nameof(startOrdinal));
        _index = startOrdinal;
    }

    public PmcroFrame Current => PmcroFrameCatalog.ByOrdinal(_index);

    public bool TryAdvance(out PmcroFrame next)
    {
        next = Current;
        if (IsHalted) return false;
        if (_index >= 24) return false;
        _index++;
        next = Current;
        return true;
    }

    public void Halt(string reason)
    {
        IsHalted = true;
        HaltReason = reason;
    }
}

public sealed class CycleErrorPath : ICycleErrorPath
{
    public Task<NextSeedIntent> OnCheckFailedAsync(PmcroRequest request, string defects, CancellationToken ct = default)
    {
        // Reflector-shaped seed; orchestrator admits on the *next* cycle only.
        var seed = new NextSeedIntent(
            Intent: "Repair defects from frame " + request.ResolvedFrameId + ": " + defects,
            SourceCycleRef: request.CycleRef ?? "",
            SourceFrameId: request.ResolvedFrameId,
            SuggestedRole: "orchestrator",
            Notes: "Emitted by REFLECT path after CHECK failure. Do not auto-execute.");
        return Task.FromResult(seed);
    }
}
