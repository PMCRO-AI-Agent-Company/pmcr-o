using Grpc.Core;
using ProjectName.Runtime;

namespace ProjectName.OrchestrationApi.Services;

public sealed class RuntimeGatewayService(RuntimeChat.RuntimeChatClient client)
{
    public async Task<ChatReply> ChatAsync(string prompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("prompt is required", nameof(prompt));

        return await client.ChatAsync(
            new ChatRequest { Prompt = prompt },
            cancellationToken: cancellationToken);
    }
}
