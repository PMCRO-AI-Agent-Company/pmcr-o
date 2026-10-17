namespace ProjectName.Application.Abstractions;

public sealed record PmcroCommandCatalog(
    string HumanRoute,
    string RawTextPolicy,
    string Init,
    IReadOnlyList<string> Commands,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Phases);

public interface IPmcroOperations
{
    PmcroCommandCatalog GetCommands();
}