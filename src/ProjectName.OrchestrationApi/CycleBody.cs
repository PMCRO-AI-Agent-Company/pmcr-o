namespace ProjectName.OrchestrationApi;

/// <summary>Legacy cycle body (intent-only). Prefer PmcroRequestEnvelope for phase-aware calls.</summary>
public sealed record CycleBody(string Intent);
