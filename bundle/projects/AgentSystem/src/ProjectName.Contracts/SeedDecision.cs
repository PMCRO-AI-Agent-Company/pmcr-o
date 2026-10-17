namespace ProjectName.Contracts;

public enum SeedDecisionKind { Accept, Reject, Resume, Defer }

public sealed record SeedDecision(
    SeedDecisionKind Kind,
    string Intent,
    string AgentRef,
    string? CycleRef,
    int? FrameOrdinal,
    string? Reason,
    string? SeedRef,
    string? Notes);

public sealed record AdmissionDecision(
    bool Admitted,
    string CycleRef,
    string Intent,
    string AgentRef,
    int StartFrameOrdinal);
