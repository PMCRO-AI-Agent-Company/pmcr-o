namespace ProjectName.Contracts;

public sealed record TrailIdentity(
    string ProjectId,
    string TrailId,
    string? ParentTrailId,
    string OwnerAgent,
    string? DelegatedBy,
    string Goal);

public sealed record SeedEnvelope(
    string SeedId,
    string TrailId,
    string TargetAgent,
    string RawIntent,
    string Status,
    string? Phase,
    bool GrantsAuthority,
    string? ParentSeedId);

public sealed record CycleIdentity(
    string CycleId,
    string SeedId,
    string TrailId,
    int Iteration);

public sealed record FrameIdentity(
    string FrameId,
    string CycleId,
    string Agent,
    string Phase,
    int Seq,
    string? IntentRef);

/// <summary>Reflector must always produce both.</summary>
public sealed record ReflectorOutcome(
    IReadOnlyList<string> EarnedConstraints,
    NextSeedIntent? NextSeed,
    string TrueIntent,
    string SourceMessyIntent,
    string CycleRef);

public sealed record NextSeedIntent(
    string Intent,
    string SourceCycleRef,
    string SourceFrameId,
    string SuggestedRole,
    string Notes,
    bool Executed = false);
