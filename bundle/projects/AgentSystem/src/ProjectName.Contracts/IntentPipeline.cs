namespace ProjectName.Contracts;

/// <summary>Ingress: raw user/API/host message. Orchestrator may accept as high-level goal.</summary>
public sealed record MessySeed(
    string RawText,
    string? Source,
    string? MessageId);

/// <summary>Egress: reflector crystallizes resource-backed true intent after a cycle.</summary>
public sealed record TrueIntent(
    string Statement,
    string SourceMessyIntent,
    string CycleRef,
    string FrameId,
    IReadOnlyList<string> ValidatedResourceIds,
    IReadOnlyList<GapRecord> GapsAddressedOrOpen);

/// <summary>Abstract agent-messaging pattern (host maps to SendMessage or equivalent).</summary>
public static class PatternCapabilities
{
    public const string AgentMessaging = "agent.messaging";
    public const string AgentDiscovery = "agent.discovery";
}
