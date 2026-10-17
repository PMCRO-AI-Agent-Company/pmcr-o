namespace ProjectName.Tooling;

using System.Text.Json;
using ModelContextProtocol.Protocol;
using ProjectName.Application.Abstractions;

public sealed record McpOperationDescriptor(
    string Name,
    string Description,
    bool ReadOnly,
    bool RequiresGrant);

public interface IMcpOperationEligibility
{
    bool IsEligible(McpOperationDescriptor operation);
}

public sealed class DefaultMcpOperationEligibility : IMcpOperationEligibility
{
    public bool IsEligible(McpOperationDescriptor operation) =>
        operation.ReadOnly && !operation.RequiresGrant;
}

public sealed class DynamicPmcroMcpProjection(
    IPmcroOperations operations,
    IMcpOperationEligibility eligibility)
{
    public const string CommandsToolName = "pmcro_commands";

    private static readonly McpOperationDescriptor CommandsOperation = new(
        CommandsToolName,
        "Return the current PMCR-O command, role, phase, and ingress routing catalog. Read-only discovery only.",
        ReadOnly: true,
        RequiresGrant: false);

    private static readonly JsonElement EmptyInputSchema = JsonDocument.Parse("""
        {"type":"object","properties":{},"additionalProperties":false}
        """).RootElement.Clone();

    public IReadOnlyList<Tool> ListTools()
    {
        if (!eligibility.IsEligible(CommandsOperation))
            return [];

        return
        [
            new Tool
            {
                Name = CommandsOperation.Name,
                Description = CommandsOperation.Description,
                InputSchema = EmptyInputSchema,
                Annotations = new ToolAnnotations
                {
                    ReadOnlyHint = true,
                    DestructiveHint = false,
                    IdempotentHint = true,
                    OpenWorldHint = false,
                },
            }
        ];
    }

    public CallToolResult Call(CallToolRequestParams request)
    {
        if (!string.Equals(request.Name, CommandsToolName, StringComparison.Ordinal))
            return Error($"unknown-tool:{request.Name}");

        if (!eligibility.IsEligible(CommandsOperation))
            return Error("tool-not-eligible");

        return new CallToolResult
        {
            Content = [new TextContentBlock { Text = JsonSerializer.Serialize(operations.GetCommands()) }],
            IsError = false,
        };
    }

    private static CallToolResult Error(string message) => new()
    {
        Content = [new TextContentBlock { Text = message }],
        IsError = true,
    };
}