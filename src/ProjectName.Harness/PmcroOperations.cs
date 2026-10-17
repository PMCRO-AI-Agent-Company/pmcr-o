namespace ProjectName.Harness;

using ProjectName.Application.Abstractions;
using ProjectName.Contracts;

public sealed class PmcroOperations : IPmcroOperations
{
    public PmcroCommandCatalog GetCommands() => new(
        HumanRoute: "@<role> /pmcro:<command>",
        RawTextPolicy: "reject-with-corrected-seed-route",
        Init: "opens Cycle 1; first cognitive frame remains ORCHESTRATE",
        Commands: PmcroConstants.BootstrapCommands.Concat(PmcroConstants.PhaseCommands).ToArray(),
        Roles: PmcroConstants.Roles.ToArray(),
        Phases: PmcroConstants.Phases.ToArray());
}