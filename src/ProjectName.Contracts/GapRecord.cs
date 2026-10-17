namespace ProjectName.Contracts;

/// <summary>Missing marketplace capability. Not invokable until create-skill + REGISTER + grant.</summary>
public sealed record GapRecord(
    string RequestedId,
    string Reason,
    string RecommendedAction,
    string CreateSkillHint);

public sealed record ResourceValidationResult(
    bool Ok,
    IReadOnlyList<string> ValidatedIds,
    IReadOnlyList<GapRecord> Gaps);
