namespace ProjectName.Harness;

using ProjectName.Contracts;

/// <summary>In-memory activation registry for tests and local auto-run.
/// Production should pin receipt digests like AgentActivationGate.</summary>
public sealed class FrameworkActivationStub : IFrameworkActivation
{
    private readonly Dictionary<string, AgentExecutionGrant> _active = new(StringComparer.Ordinal);

    public Task<AgentExecutionGrant> ActivateAsync(
        string frameworkId,
        AgentActivationReceipt receipt,
        AgentExecutionGrant ceiling,
        CancellationToken ct = default)
    {
        if (receipt.Verdict != "PASS")
            throw new UnauthorizedAccessException("Activation requires PASS receipt");
        if (receipt.MakerIdentity == receipt.CheckerIdentity)
            throw new UnauthorizedAccessException("Maker and Checker identities must differ");
        if (receipt.CheckerIdentity is not ("pmcro.checker" or "pmcro-checker"))
            throw new UnauthorizedAccessException("Checker identity not accepted: " + receipt.CheckerIdentity);

        var effective = new AgentExecutionGrant(
            receipt.Grant.DelegatedGrant.Intersect(ceiling.DelegatedGrant, StringComparer.Ordinal).ToArray(),
            receipt.Grant.HostMaximum.Intersect(ceiling.HostMaximum, StringComparer.Ordinal).ToArray(),
            receipt.Grant.EnvironmentMaximum.Intersect(ceiling.EnvironmentMaximum, StringComparer.Ordinal).ToArray(),
            receipt.Grant.Denies.Union(ceiling.Denies, StringComparer.Ordinal).ToArray(),
            "activation-stub:" + frameworkId);

        _active[frameworkId] = effective;
        return Task.FromResult(effective);
    }

    public bool IsActivated(string frameworkId) => _active.ContainsKey(frameworkId);

    public AgentExecutionGrant? GetGrant(string frameworkId) =>
        _active.TryGetValue(frameworkId, out var g) ? g : null;
}
