namespace ProjectName.Harness;
using ProjectName.Contracts;

// Production hosts without operator trust are explicitly locked, never backed by the test stub.
public sealed class DisabledProductionActivation : IFrameworkActivation
{
    public bool IsActivated(string frameworkId) => false;
    public Task<AgentExecutionGrant> ActivateAsync(string frameworkId, AgentActivationReceipt receipt,
        AgentExecutionGrant ceiling, CancellationToken ct = default) =>
        throw new UnauthorizedAccessException("operator-control-plane-trust-required");
}
