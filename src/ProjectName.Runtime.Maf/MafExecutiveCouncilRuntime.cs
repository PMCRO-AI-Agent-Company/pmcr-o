namespace ProjectName.Runtime.Maf;

using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using Microsoft.Agents.AI.Workflows.InProc;
using ProjectName.Contracts;

public static class MafExecutiveHumanDecisionBridge
{
    public static PmcroMessageEnvelope ToMessage(
        RequestInfoEvent requestEvent,
        string trailId,
        string sourceAgent,
        string targetAgent,
        string? seedRef = null,
        string? causationId = null,
        string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(requestEvent);
        ArgumentException.ThrowIfNullOrWhiteSpace(trailId);

        var request = requestEvent.Request;
        var payload = JsonSerializer.Serialize(new
        {
            requestId = request.RequestId,
            portId = request.PortInfo.PortId,
            requestType = request.PortInfo.RequestType.ToString(),
            responseType = request.PortInfo.ResponseType.ToString(),
            data = request.Data.ToString()
        });

        return new PmcroMessageEnvelope(
            Guid.NewGuid().ToString("D"),
            PmcroMessageKinds.HumanDecisionRequest,
            trailId,
            sourceAgent,
            targetAgent,
            payload,
            DateTimeOffset.UtcNow,
            SeedRef: seedRef,
            CausationId: causationId ?? request.RequestId,
            CorrelationId: correlationId ?? trailId);
    }
}

public sealed class MafExecutiveCheckpointStore : IDisposable
{
    private readonly FileSystemJsonCheckpointStore _store;

    public MafExecutiveCheckpointStore(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        var directory = Directory.CreateDirectory(Path.GetFullPath(root));
        Root = directory.FullName;
        _store = new FileSystemJsonCheckpointStore(directory);
        Manager = CheckpointManager.CreateJson(_store, customOptions: null);
    }

    public string Root { get; }
    public CheckpointManager Manager { get; }

    public ValueTask<CheckpointInfo?> GetLatestAsync(string sessionId, CancellationToken ct = default) =>
        Manager.GetLatestCheckpointAsync(sessionId, ct);

    public void Dispose() => _store.Dispose();
}

public sealed class MafExecutiveWorkflowRuntime : IDisposable
{
    private readonly MafExecutiveCheckpointStore _checkpoints;
    private readonly IWorkflowExecutionEnvironment _environment;

    public MafExecutiveWorkflowRuntime(string checkpointRoot)
    {
        _checkpoints = new MafExecutiveCheckpointStore(checkpointRoot);
        _environment = InProcessExecution.Default.WithCheckpointing(_checkpoints.Manager);
    }

    public bool IsCheckpointingEnabled => _environment.IsCheckpointingEnabled;
    public string CheckpointRoot => _checkpoints.Root;

    public ValueTask<Run> RunAsync<TInput>(Workflow workflow, TInput input, string sessionId, CancellationToken ct = default)
        where TInput : notnull => _environment.RunAsync(workflow, input, sessionId, ct);

    public ValueTask<Run> ResumeAsync(Workflow workflow, CheckpointInfo checkpoint, CancellationToken ct = default) =>
        _environment.ResumeAsync(workflow, checkpoint, ct);

    public ValueTask<CheckpointInfo?> GetLatestCheckpointAsync(string sessionId, CancellationToken ct = default) =>
        _checkpoints.GetLatestAsync(sessionId, ct);

    public void Dispose() => _checkpoints.Dispose();
}
