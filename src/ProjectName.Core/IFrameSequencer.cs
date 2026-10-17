namespace ProjectName.Core;

using ProjectName.Contracts;

/// <summary>Advances ordered frames within one admitted Cycle. Does not open a new Cycle.</summary>
public interface IFrameSequencer
{
    PmcroFrame Current { get; }
    bool TryAdvance(out PmcroFrame next);
    void Halt(string reason);
    bool IsHalted { get; }
    string? HaltReason { get; }
}

public interface ICycleErrorPath
{
    /// <summary>CHECK failure → REFLECT NextSeed only; never auto-execute.</summary>
    Task<NextSeedIntent> OnCheckFailedAsync(PmcroRequest request, string defects, CancellationToken ct = default);
}

public sealed record NextSeedIntent(
    string Intent,
    string SourceCycleRef,
    string SourceFrameId,
    string SuggestedRole,
    string Notes);
