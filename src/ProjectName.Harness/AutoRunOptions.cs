namespace ProjectName.Harness;

/// <summary>
/// Continuous / auto-run mode (Claude-like accept-edits loop) under PMCR-O law.
/// Auto-run never skips CHECK, never self-grants, never activates a framework silently.
/// </summary>
public sealed class AutoRunOptions
{
    /// <summary>When true, admitted MAKE may continue without human keypress until CHECK or HALT.</summary>
    public bool ContinuousWithinPhase { get; init; }

    /// <summary>Max tool/MCP steps per phase before mandatory CHECK or pause.</summary>
    public int MaxStepsPerPhase { get; init; } = 32;

    /// <summary>Require independent Checker identity ≠ Maker before any PASS.</summary>
    public bool RequireIndependentCheck { get; init; } = true;

    /// <summary>Framework must already be activated; auto-run does not call Activate.</summary>
    public bool RequirePriorFrameworkActivation { get; init; } = true;

    /// <summary>Stop on first FAIL/BLOCKED; emit Reflector NextSeedIntent only, never auto-execute.</summary>
    public bool HaltOnFail { get; init; } = true;
}
