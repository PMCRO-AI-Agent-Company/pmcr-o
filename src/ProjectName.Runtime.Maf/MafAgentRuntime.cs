using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ProjectName.Runtime.Abstractions;

namespace ProjectName.Runtime.Maf;

public sealed class MafAgentRuntime(
    IChatClient chatClient,
    IMafGovernanceBoundary governance,
    IMafToolProvider? toolProvider = null) : IAgentRuntime
{
    public string FrameworkId => "maf";

    public async Task<AgentRuntimeResult> RunAsync(
        AgentRuntimeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.AgentId))
            throw new ArgumentException("agent-id-required", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Instructions))
            throw new ArgumentException("instructions-required", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Input))
            throw new ArgumentException("input-required", nameof(request));
        if (request.EffectiveGrant.Count == 0)
            throw new UnauthorizedAccessException("effective-grant-required");

        var tools = toolProvider is null
            ? Array.Empty<AITool>()
            : (await toolProvider.GetToolsAsync(cancellationToken)).ToArray();

        AIAgent agent = chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Id = request.AgentId,
            Name = request.AgentId,
            ChatOptions = new ChatOptions
            {
                Instructions = request.Instructions,
                MaxOutputTokens = request.MaxOutputTokens,
                Tools = tools,
            },
        });

        agent = governance.Govern(agent, request.AgentId, request.EffectiveGrant, request.Delegated);
        var response = await agent.RunAsync(
            new ChatMessage(ChatRole.User, request.Input),
            cancellationToken: cancellationToken);

        return new AgentRuntimeResult(request.AgentId, response.ToString(), FrameworkId);
    }
}