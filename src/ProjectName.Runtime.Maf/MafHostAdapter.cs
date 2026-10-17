namespace ProjectName.Runtime.Maf;

using ProjectName.Contracts;
using ProjectName.Runtime.Abstractions;

/// <summary>
/// MAF-native host: execution is Microsoft Agent Framework + ProjectName services.
/// .agents remains the portable convention; this adapter does not own marketplace truth.
/// </summary>
public sealed class MafHostAdapter : IHostFrameworkAdapter
{
    public string FrameworkId => "maf";

    public HostProjection DescribeProjection(string agentIdentity) => new(
        FrameworkId,
        ".agents/agents/" + agentIdentity + ".md",
        ".agents/skills",
        "MAF-native runtime; projection is .agents; activation is explicit");

    public string FormatInvocation(PmcroRequest request) =>
        request.Route ?? ("/pmcro:" + request.Command);
}
