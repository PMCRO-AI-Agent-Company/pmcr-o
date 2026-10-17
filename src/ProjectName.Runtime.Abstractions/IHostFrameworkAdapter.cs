namespace ProjectName.Runtime.Abstractions;

using ProjectName.Contracts;

/// <summary>
/// One adapter per host framework. Keep each host; activate explicitly.
/// Supported bootstrap ids align with existing HostAdapter: Claude, OpenAI, MAF, Cursor, GitHub, MCP, xAI/Grok.
/// </summary>
public interface IHostFrameworkAdapter
{
    string FrameworkId { get; }

    /// <summary>Projection paths only (e.g. .claude/agents). Never grants authority.</summary>
    HostProjection DescribeProjection(string agentIdentity);

    /// <summary>Map PmcroRequest to host-native invocation string (e.g. Claude subagent or @role).</summary>
    string FormatInvocation(PmcroRequest request);
}

public sealed record HostProjection(
    string FrameworkId,
    string AgentsPath,
    string SkillsPath,
    string StateNote);
