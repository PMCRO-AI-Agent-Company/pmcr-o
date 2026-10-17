namespace ProjectName.Contracts;

/// <summary>Canonical roles and phases for the PMCR-O template. Do not rename casually.</summary>
public static class PmcroConstants
{
    public static readonly string[] Roles = ["orchestrator", "planner", "maker", "checker", "reflector"];
    public static readonly string[] Phases = ["ORCHESTRATE", "PLAN", "MAKE", "CHECK", "REFLECT"];
    public static readonly string[] BootstrapCommands = ["init", "seed"];
    public static readonly string[] PhaseCommands = ["orchestrate", "plan", "make", "check", "reflect"];

    public static string IdentityForRole(string role) => "pmcro-" + role;
    public static string DidForRole(string role) => "did:mesh:pmcro-" + role;

    public static string PhaseForCommand(string command) => command.ToUpperInvariant() switch
    {
        "ORCHESTRATE" or "PLAN" or "MAKE" or "CHECK" or "REFLECT" => command.ToUpperInvariant(),
        "INIT" or "SEED" => "", // bootstrap, not a cognitive phase
        _ => throw new ArgumentOutOfRangeException(nameof(command), command, "Unknown command")
    };

    public static bool CommandRequiresCycle(string command) =>
        command is "plan" or "make" or "check" or "reflect";
}
