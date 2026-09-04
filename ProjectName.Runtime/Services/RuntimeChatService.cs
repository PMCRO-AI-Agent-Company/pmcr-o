using Grpc.Core;
using Microsoft.Extensions.AI;

namespace ProjectName.Runtime.Services;

public sealed class RuntimeChatService(
    IChatClient chatClient,
    ILogger<RuntimeChatService> logger) : RuntimeChat.RuntimeChatBase
{
    private const string Model = "qwen3:8b";

    public override async Task<ChatReply> Chat(
        ChatRequest request,
        ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "prompt is required"));

        logger.LogInformation("Runtime chat request received for model {Model}", Model);

        try
        {
            var response = await chatClient.GetResponseAsync(
                $"/no_think\n{request.Prompt}",
                cancellationToken: context.CancellationToken);

            return new ChatReply
            {
                Model = Model,
                Response = response.Text ?? response.ToString()
            };
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Runtime chat request was canceled by the caller.");
            throw new RpcException(new Status(StatusCode.Cancelled, "The chat request was canceled by the caller."));
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex, "Runtime chat request timed out or was canceled while contacting Ollama.");
            throw new RpcException(new Status(StatusCode.DeadlineExceeded, "The Ollama chat request exceeded its deadline."));
        }
    }
}
