namespace ProjectName.AgentCli;

using ProjectName.Application.Abstractions;
using Spectre.Console;

public sealed class TerminalUi(IPmcroOperations operations)
{
    public void Render()
    {
        var catalog = operations.GetCommands();

        AnsiConsole.Write(new Rule("[bold]ProjectName AgentSystem[/]").LeftJustified());
        AnsiConsole.Write(new Panel(Markup.Escape(catalog.HumanRoute))
            .Header("PMCR-O route")
            .Expand());

        var table = new Table().RoundedBorder();
        table.AddColumn("[bold]Dimension[/]");
        table.AddColumn("[bold]Values[/]");
        table.AddRow("Commands", Markup.Escape(string.Join(", ", catalog.Commands)));
        table.AddRow("Roles", Markup.Escape(string.Join(", ", catalog.Roles)));
        table.AddRow("Phases", Markup.Escape(string.Join(", ", catalog.Phases)));
        AnsiConsole.Write(table);

        var policy = new Tree("[bold]Runtime policy[/]");
        policy.AddNode($"Raw text: {Markup.Escape(catalog.RawTextPolicy)}");
        policy.AddNode($"Init: {Markup.Escape(catalog.Init)}");
        AnsiConsole.Write(policy);
    }
}