using ModelContextProtocol.Server;

namespace ProjectName.Capabilities;

public static class McpSdkBoundary
{
    public static Type ServerOptionsType => typeof(McpServerOptions);
    public static string AssemblyName => typeof(McpServerOptions).Assembly.GetName().Name ?? "ModelContextProtocol";
}
