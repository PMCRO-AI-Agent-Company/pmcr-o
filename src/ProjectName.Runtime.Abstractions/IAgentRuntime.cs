namespace ProjectName.Runtime.Abstractions;

public sealed record AgentRuntimeRequest(
    string AgentId,
    string Instructions,
    string Input,
    IReadOnlySet<string> EffectiveGrant,
    bool Delegated,
    int MaxOutputTokens = 2048);

public sealed record AgentRuntimeResult(
    string AgentId,
    string Text,
    string FrameworkId);

public interface IAgentRuntime
{
    string FrameworkId { get; }
    Task<AgentRuntimeResult> RunAsync(AgentRuntimeRequest request, CancellationToken cancellationToken = default);
}
