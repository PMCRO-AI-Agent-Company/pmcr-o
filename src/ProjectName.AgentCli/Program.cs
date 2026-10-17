using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjectName.Application.Abstractions;
using ProjectName.AgentCli;
using ProjectName.Harness;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddProjectNameAgentSystem();
using var host = builder.Build();
var operations = host.Services.GetRequiredService<IPmcroOperations>();

if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
{
    Console.WriteLine("ProjectName Agent CLI");
    Console.WriteLine("  commands [--json]   Show PMCR-O commands, roles, phases, and route syntax.");
    Console.WriteLine("  ui                  Render the rich terminal projection.");
    Console.WriteLine("  control-plane ...   Run the Trail-bound production control-plane command.");
    return;
}

if (args[0].Equals("control-plane", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await ControlPlaneCommand.RunAsync(args.Skip(1).ToArray());
    return;
}

if (args[0].Equals("ui", StringComparison.OrdinalIgnoreCase))
{
    new TerminalUi(operations).Render();
    return;
}

if (args[0].Equals("commands", StringComparison.OrdinalIgnoreCase))
{
    var catalog = operations.GetCommands();
    if (args.Skip(1).Any(x => x.Equals("--json", StringComparison.OrdinalIgnoreCase)))
    {
        Console.WriteLine(JsonSerializer.Serialize(catalog, new JsonSerializerOptions { WriteIndented = true }));
        return;
    }

    Console.WriteLine(catalog.HumanRoute);
    Console.WriteLine($"Raw text: {catalog.RawTextPolicy}");
    Console.WriteLine($"Init: {catalog.Init}");
    Console.WriteLine("Commands: " + string.Join(", ", catalog.Commands));
    Console.WriteLine("Roles: " + string.Join(", ", catalog.Roles));
    Console.WriteLine("Phases: " + string.Join(", ", catalog.Phases));
    return;
}

Console.Error.WriteLine($"unknown-command:{args[0]}");
Environment.ExitCode = 2;
