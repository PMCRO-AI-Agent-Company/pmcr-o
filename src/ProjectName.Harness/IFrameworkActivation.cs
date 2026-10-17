namespace ProjectName.Harness;

using ProjectName.Contracts;

/// <summary>Explicit per-host activation. Projection is not activation.</summary>
public interface IFrameworkActivation
{
    /// <summary>Activate one framework under a grant ceiling. Returns effective grant (intersected).</summary>
    Task<AgentExecutionGrant> ActivateAsync(
        string frameworkId,
        AgentActivationReceipt receipt,
        AgentExecutionGrant ceiling,
        CancellationToken ct = default);

    bool IsActivated(string frameworkId);
}
