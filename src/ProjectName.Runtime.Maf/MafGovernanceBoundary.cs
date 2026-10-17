using AgentGovernance;
using AgentGovernance.Extensions.Microsoft.Agents;
using AgentGovernance.Policy;
using Microsoft.Agents.AI;

namespace ProjectName.Runtime.Maf;

public interface IMafGovernanceBoundary
{
    AIAgent Govern(AIAgent agent, string agentId, IReadOnlySet<string> effectiveGrant, bool delegated);
}

public sealed class AgentGovernanceBoundary : IMafGovernanceBoundary
{
    private readonly GovernanceKernel _kernel;

    public AgentGovernanceBoundary(string policyPath)
    {
        if (string.IsNullOrWhiteSpace(policyPath) || !File.Exists(policyPath))
            throw new FileNotFoundException("governance-policy-required", policyPath);

        _kernel = new GovernanceKernel(new GovernanceOptions
        {
            PolicyPaths = new() { policyPath },
            ConflictStrategy = ConflictResolutionStrategy.DenyOverrides,
            EnableRings = true,
            EnablePromptInjectionDetection = true,
            EnableCircuitBreaker = true,
        });
    }

    public AIAgent Govern(AIAgent agent, string agentId, IReadOnlySet<string> effectiveGrant, bool delegated)
    {
        ArgumentNullException.ThrowIfNull(agent);
        if (string.IsNullOrWhiteSpace(agentId)) throw new ArgumentException("agent-id-required", nameof(agentId));
        if (effectiveGrant.Count == 0) throw new UnauthorizedAccessException("effective-grant-required");

        return agent.WithGovernance(_kernel, new AgentFrameworkGovernanceOptions
        {
            DefaultAgentId = agentId,
            AgentIdResolver = (_, _) => agentId,
            EnableFunctionMiddleware = false,
            BlockedRunResponseFactory = decision => throw new InvalidOperationException(
                "Agent run blocked by governance: " + decision.Reason),
        });
    }
}
