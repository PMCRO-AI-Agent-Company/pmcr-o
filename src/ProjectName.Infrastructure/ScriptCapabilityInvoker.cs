namespace ProjectName.Infrastructure;

using System.Diagnostics;
using ProjectName.Application.Abstractions;
using ProjectName.Capabilities;
using ProjectName.Contracts;

public sealed class ScriptCapabilityInvoker(ICapabilityResolver resolver) : ICapabilityInvoker
{
    public async Task<CapabilityInvocationResult> InvokeAsync(
        CapabilityInvocationRequest request,
        AgentExecutionGrant grant,
        CancellationToken ct = default)
    {
        var started = DateTimeOffset.UtcNow;
        if (!File.Exists(request.ScriptPath))
            return Result(request, false, "script-missing", 2, "", "Script not found", started);

        var mode = request.Mode.ToLowerInvariant();
        if (mode is not ("preflight" or "simulate" or "execute" or "check"))
            return Result(request, false, "invalid-mode", 2, "", mode, started);

        if (mode == "preflight")
            return Result(request, true, "preflight-pass", 0, request.ScriptPath, "", started);

        if (!resolver.IsGranted("capability.invoke", grant) ||
            !resolver.IsGranted(request.CapabilityId, grant))
            return Result(request, false, "capability-not-granted", 3, "", "Effective grant denied invocation", started);

        if (mode == "simulate")
            return Result(request, true, "simulation-pass", 0, request.ScriptPath, "", started);

        if (mode == "check")
            return Result(request, true, "check-pass", 0, request.ScriptPath, "", started);

        ProcessStartInfo psi;
        try { psi = BuildProcess(request); }
        catch (NotSupportedException ex)
        {
            return Result(request, false, "unsupported-script-type", 4, "", ex.Message, started);
        }

        using var process = new Process { StartInfo = psi };
        process.Start();
        var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);
        var stdout = await stdoutTask;
        var stderr = await stderrTask;
        return Result(request, process.ExitCode == 0,
            process.ExitCode == 0 ? "executed" : "execution-failed",
            process.ExitCode, stdout, stderr, started);
    }

    private static ProcessStartInfo BuildProcess(CapabilityInvocationRequest request)
    {
        var extension = Path.GetExtension(request.ScriptPath).ToLowerInvariant();
        var psi = extension switch
        {
            ".ps1" => New("powershell.exe", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", request.ScriptPath),
            ".cmd" or ".bat" => New("cmd.exe", "/c", request.ScriptPath),
            ".py" => New(ResolvePython(), request.ScriptPath),
            _ => throw new NotSupportedException($"Unsupported script type: {extension}")
        };
        foreach (var arg in request.Arguments ?? []) psi.ArgumentList.Add(arg);
        return psi;
    }

    private static ProcessStartInfo New(string fileName, params string[] args)
    {
        var psi = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var arg in args) psi.ArgumentList.Add(arg);
        return psi;
    }

    private static string ResolvePython() =>
        Environment.GetEnvironmentVariable("AGENTSYSTEM_PYTHON") ?? "python";

    private static CapabilityInvocationResult Result(CapabilityInvocationRequest request,
        bool ok, string status, int exitCode, string stdout, string stderr, DateTimeOffset started) =>
        new(ok, status, request.CapabilityId, request.ScriptPath, request.Mode,
            exitCode, stdout, stderr, started, DateTimeOffset.UtcNow);
}
