namespace ProjectName.Harness;

using ProjectName.Contracts;

public sealed record PhaseHandlerResult(
    string Status,
    IReadOnlyList<string> EvidenceRefs,
    bool GrantsAuthority = false);

public interface IPhaseHandler
{
    Task<PhaseHandlerResult> HandleAsync(PmcroRequest request, string frameId, CancellationToken ct = default);
}

public sealed class DefaultPhaseHandler : IPhaseHandler
{
    public Task<PhaseHandlerResult> HandleAsync(PmcroRequest request, string frameId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(frameId);
        return Task.FromResult(new PhaseHandlerResult(
            "handled frame " + frameId,
            Array.Empty<string>(),
            GrantsAuthority: false));
    }
}
