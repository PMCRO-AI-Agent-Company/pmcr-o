namespace ProjectName.Contracts;

public sealed record CapabilityInvocationRequest(
    string CapabilityId,
    string ScriptPath,
    string Mode = "simulate",
    IReadOnlyList<string>? Arguments = null,
    string? Caller = null);

public sealed record CapabilityInvocationResult(
    bool Ok,
    string Status,
    string CapabilityId,
    string ScriptPath,
    string Mode,
    int ExitCode,
    string StandardOutput,
    string StandardError,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt);
