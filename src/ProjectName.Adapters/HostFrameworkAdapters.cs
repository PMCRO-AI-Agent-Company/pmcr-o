namespace ProjectName.Adapters;

using ProjectName.Contracts;
using ProjectName.Runtime.Abstractions;

public sealed class ClaudeCodeHostAdapter : IHostFrameworkAdapter
{
    public string FrameworkId => "claude-code";
    public HostProjection DescribeProjection(string agentIdentity) => new(
        FrameworkId,
        ".claude/agents/" + agentIdentity + ".md",
        ".claude/skills",
        "projection only; activate separately");
    public string FormatInvocation(PmcroRequest request) =>
        request.Route ?? ("@" + RoleFrom(request) + " /pmcro:" + request.Command);
    private static string RoleFrom(PmcroRequest r) => r.Command switch
    {
        "plan" => "planner", "make" => "maker", "check" => "checker", "reflect" => "reflector",
        _ => "orchestrator"
    };
}

public sealed class GrokHostAdapter : IHostFrameworkAdapter
{
    public string FrameworkId => "grok";
    public HostProjection DescribeProjection(string agentIdentity) => new(
        FrameworkId, ".grok/agents/" + agentIdentity + ".md", ".grok/skills",
        "unverified adapter; projection only");
    public string FormatInvocation(PmcroRequest request) =>
        request.Route ?? ("@" + request.AgentRef + " /pmcro:" + request.Command);
}

public sealed class CursorHostAdapter : IHostFrameworkAdapter
{
    public string FrameworkId => "cursor";
    public HostProjection DescribeProjection(string agentIdentity) => new(
        FrameworkId, ".cursor/agents/" + agentIdentity + ".md", ".agents/skills",
        "projection only");
    public string FormatInvocation(PmcroRequest request) =>
        request.Route ?? ("@" + request.AgentRef + " /pmcro:" + request.Command);
}

public sealed class CodexHostAdapter : IHostFrameworkAdapter
{
    public string FrameworkId => "codex";
    public HostProjection DescribeProjection(string agentIdentity) => new(
        FrameworkId, ".agents/agents/" + agentIdentity + ".md", ".agents/skills",
        "native skills discovery; no custom-agent parity assumed");
    public string FormatInvocation(PmcroRequest request) =>
        request.Route ?? ("@" + request.AgentRef + " /pmcro:" + request.Command);
}
