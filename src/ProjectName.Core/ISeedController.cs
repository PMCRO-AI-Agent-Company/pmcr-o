namespace ProjectName.Core;

using ProjectName.Contracts;

public interface ISeedController
{
    AdmissionDecision Accept(SeedDecision decision);
    void Reject(SeedDecision decision);
    ResumeAck Resume(SeedDecision decision);
    void Defer(SeedDecision decision);
}

public sealed record ResumeAck(string CycleRef, int FrameOrdinal, string FrameId);
