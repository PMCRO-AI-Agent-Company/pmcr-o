using ProjectName.Capabilities;
using ProjectName.Runtime.Abstractions;
using ProjectName.Runtime.Maf;
using Xunit;

namespace ProjectName.Runtime.Tests;

public sealed class ConventionAlignmentTests
{
    [Fact]
    public void MafRuntime_ImplementsPortableRuntimeBoundary()
    {
        Assert.Contains(typeof(IAgentRuntime), typeof(MafAgentRuntime).GetInterfaces());
        var ctor = Assert.Single(typeof(MafAgentRuntime).GetConstructors());
        var parameters = ctor.GetParameters();
        Assert.Equal("Microsoft.Extensions.AI.IChatClient", parameters[0].ParameterType.FullName);
        Assert.Equal(typeof(IMafGovernanceBoundary), parameters[1].ParameterType);
    }

    [Fact]
    public void OfficialMcpSdk_IsTheCapabilityProtocolBoundary()
    {
        Assert.Equal("ModelContextProtocol.Core", McpSdkBoundary.AssemblyName);
        Assert.Equal("ModelContextProtocol.Server.McpServerOptions", McpSdkBoundary.ServerOptionsType.FullName);
    }

    [Fact]
    public void GovernanceBoundary_ReferencesOfficialAgentGovernancePackages()
    {
        var refs = typeof(AgentGovernanceBoundary).Assembly.GetReferencedAssemblies().Select(x => x.Name ?? "").ToHashSet();
        Assert.Contains("AgentGovernance", refs);
        Assert.Contains(refs, x => x.Contains("AgentGovernance.Extensions.Microsoft.Agents", StringComparison.Ordinal));
    }

    [Fact]
    public void ServiceDefaults_AreSharedInfrastructure_NotPmcroLaw()
    {
        Assert.Equal("Microsoft.Extensions.Hosting.ProjectNameServiceDefaults",
            typeof(Microsoft.Extensions.Hosting.ProjectNameServiceDefaults).FullName);
    }
}
