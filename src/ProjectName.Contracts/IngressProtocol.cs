namespace ProjectName.Contracts;

public sealed record PmcroInitRequest(
    string Intent,
    string AgentRef = "orchestrator",
    string? ProjectId = null,
    string? TrailRef = null,
    string? ParentTrailRef = null);

public sealed record PmcroInitAcknowledgement(
    string Route,
    string Status,
    string ProjectId,
    string TrailId,
    string SeedId,
    string CycleId,
    int CycleNumber,
    string Intent,
    string NextRoute);

public sealed record PmcroIngressRejection(
    string Status,
    string Reason,
    string Received,
    string CorrectedRoute,
    string Guidance,
    bool Admitted = false);


public sealed record PmcroFirstResponse(
    string Route,
    string Command,
    string Intent,
    string AgentRef,
    string Status,
    bool AdmittedByResponse);

public sealed record ParsedPmcroRoute(
    string Agent,
    string Command,
    string? Intent,
    string Original);
