namespace ProjectName.Core;

using ProjectName.Contracts;

/// <summary>Orchestrator-only seed controls. Not cognitive phases.</summary>
public sealed class SeedController : ISeedController
{
    private readonly Dictionary<string, int> _cycles = new(StringComparer.Ordinal);
    private int _seq;

    public AdmissionDecision Accept(SeedDecision decision)
    {
        if (decision.Kind != SeedDecisionKind.Accept)
            throw new ArgumentException("Kind must be Accept");
        if (string.IsNullOrWhiteSpace(decision.Intent))
            throw new ArgumentException("intent required");
        var cycle = decision.CycleRef;
        if (string.IsNullOrWhiteSpace(cycle))
        {
            _seq++;
            cycle = "cycle-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + _seq;
        }
        var start = decision.FrameOrdinal ?? 0;
        _cycles[cycle] = start;
        return new AdmissionDecision(true, cycle, decision.Intent, decision.AgentRef, start);
    }

    public void Reject(SeedDecision decision)
    {
        if (decision.Kind != SeedDecisionKind.Reject)
            throw new ArgumentException("Kind must be Reject");
        if (string.IsNullOrWhiteSpace(decision.Reason))
            throw new ArgumentException("reason required for reject");
    }

    public ResumeAck Resume(SeedDecision decision)
    {
        if (decision.Kind != SeedDecisionKind.Resume)
            throw new ArgumentException("Kind must be Resume");
        if (string.IsNullOrWhiteSpace(decision.CycleRef))
            throw new ArgumentException("cycleRef required for resume");
        var ordinal = decision.FrameOrdinal
            ?? (_cycles.TryGetValue(decision.CycleRef, out var o) ? o : 0);
        if (ordinal < 0 || ordinal > 24)
            throw new ArgumentOutOfRangeException(nameof(decision.FrameOrdinal));
        _cycles[decision.CycleRef] = ordinal;
        var frame = PmcroFrameCatalog.ByOrdinal(ordinal);
        return new ResumeAck(decision.CycleRef, ordinal, frame.FrameId);
    }

    public void Defer(SeedDecision decision)
    {
        if (decision.Kind != SeedDecisionKind.Defer)
            throw new ArgumentException("Kind must be Defer");
    }
}
