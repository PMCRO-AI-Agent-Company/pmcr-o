namespace ProjectName.Harness;

using ProjectName.Contracts;

/// <summary>
/// Dispatches one Frame (role × phase). Does not skip frames or open cycles.
/// Subagent parent_tool_use_id cannot run ORCHESTRATE/INIT/SEED.
/// </summary>
public sealed class PhaseRouter : IPhaseRouter
{
    private readonly IFrameworkActivation _activation;
    private readonly AutoRunOptions _autoRun;
    private readonly IPhaseHandler _handler;

    public PhaseRouter(
        IFrameworkActivation activation,
        AutoRunOptions? autoRun = null,
        IPhaseHandler? handler = null)
    {
        _activation = activation;
        _autoRun = autoRun ?? new AutoRunOptions();
        _handler = handler ?? new DefaultPhaseHandler();
    }

    public async Task<object> DispatchAsync(PmcroRequest request, CancellationToken ct = default)
    {
        Validate(request);

        if (_autoRun.RequirePriorFrameworkActivation
            && !string.IsNullOrEmpty(request.Framework)
            && !_activation.IsActivated(request.Framework))
            throw new UnauthorizedAccessException(
                "Framework not activated: " + request.Framework);

        if (!string.IsNullOrEmpty(request.ParentToolUseId)
            && request.ResolvedPhase is "ORCHESTRATE" or ""
            && request.Command is "orchestrate" or "seed" or "init")
            throw new UnauthorizedAccessException(
                "Subagent context cannot admit seeds or ORCHESTRATE/INIT.");

        var frameId = request.ResolvedFrameId;
        var handled = await _handler.HandleAsync(request, frameId, ct);
        if (handled.GrantsAuthority)
            throw new UnauthorizedAccessException("phase-handler-cannot-grant-authority");

        return new PhaseDispatchAck(
            request.Command,
            request.AgentRef,
            request.CycleRef,
            request.Framework,
            handled.Status,
            request.ResolvedRole,
            request.ResolvedPhase,
            frameId,
            handled.EvidenceRefs,
            handled.GrantsAuthority);
    }

    private static void Validate(PmcroRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Intent))
            throw new ArgumentException("intent required");
        if (string.IsNullOrWhiteSpace(request.AgentRef))
            throw new ArgumentException("agentRef required");

        var role = request.ResolvedRole;
        var phase = request.ResolvedPhase;

        if (!string.IsNullOrEmpty(phase) && !PmcroConstants.Phases.Contains(phase))
            throw new ArgumentException("unknown phase: " + phase);
        if (!PmcroConstants.Roles.Contains(role) && !role.StartsWith("project", StringComparison.OrdinalIgnoreCase))
        {
            if (request.Role is not null && !PmcroConstants.Roles.Contains(request.Role))
                throw new ArgumentException("unknown role: " + request.Role);
        }

        if (PmcroConstants.CommandRequiresCycle(request.Command)
            && string.IsNullOrWhiteSpace(request.CycleRef))
            throw new ArgumentException("cycleRef required for " + request.Command);

        if (PmcroConstants.Roles.Contains(role) && !string.IsNullOrEmpty(phase))
        {
            var id = role + ":" + phase;
            if (PmcroFrameCatalog.ById(id) is null)
                throw new ArgumentException("invalid frame: " + id);
        }
    }
}

public sealed record PhaseDispatchAck(
    string Command,
    string AgentRef,
    string? CycleRef,
    string? Framework,
    string Status,
    string? Role = null,
    string? Phase = null,
    string? FrameId = null,
    IReadOnlyList<string>? EvidenceRefs = null,
    bool GrantsAuthority = false);
