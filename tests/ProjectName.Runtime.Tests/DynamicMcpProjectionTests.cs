namespace ProjectName.Runtime.Tests;

using Xunit;

using ProjectName.Harness;
using ProjectName.Tooling;

public sealed class DynamicMcpProjectionTests
{
    [Fact]
    public void Default_projection_exposes_only_read_only_command_discovery()
    {
        var projection = new DynamicPmcroMcpProjection(
            new PmcroOperations(),
            new DefaultMcpOperationEligibility());

        var tools = projection.ListTools();

        var tool = Assert.Single(tools);
        Assert.Equal(DynamicPmcroMcpProjection.CommandsToolName, tool.Name);
        Assert.True(tool.Annotations?.ReadOnlyHint);
        Assert.DoesNotContain(tools, x => x.Name.Contains("init", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(tools, x => x.Name.Contains("request", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Eligibility_can_remove_tools_without_changing_projection_code()
    {
        var projection = new DynamicPmcroMcpProjection(
            new PmcroOperations(),
            new DenyAllEligibility());

        Assert.Empty(projection.ListTools());
    }

    private sealed class DenyAllEligibility : IMcpOperationEligibility
    {
        public bool IsEligible(McpOperationDescriptor operation) => false;
    }
}