namespace ProjectName.Harness;

using System.Text.Json;
using ProjectName.Contracts;
using ProjectName.Core;

public static class CommissioningEvidenceValidator
{
    public static void ValidateMutation(
        LifecycleActionRequest action,
        string operation,
        string? expectedCandidateDigest = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (!StringComparer.Ordinal.Equals(action.Operation, operation))
            throw new UnauthorizedAccessException("trail-operation-mismatch");

        ValidateCommonTrailEvidence(action, expectedCandidateDigest);

        if (TryValidateNormalGrantBoundary(action.GrantVerificationRef)) return;

throw new UnauthorizedAccessException("verified-grant-boundary-required");
    }

    internal static void ValidateCommonTrailEvidence(LifecycleActionRequest action, string? expectedCandidateDigest)
    {
        Require(action.TrailId, "trail-ref-required");
        Require(action.SeedRef, "seed-ref-required");
        Require(action.CausationId, "causation-id-required");
        Require(action.CorrelationId, "correlation-id-required");
        Require(action.AuthorizationRef, "authorization-ref-required");
        Require(action.CheckerVerdictRef, "checker-verdict-ref-required");
        Require(action.ProvenanceRef, "provenance-ref-required");
        Require(action.AdmissionRef, "admission-ref-required");
        Require(action.GrantVerificationRef, "grant-verification-ref-required");
        Require(action.CapabilityId, "capability-id-required");
        Require(action.SourceEventId, "source-event-id-required");

        var authorization = ReadObject(action.AuthorizationRef, "authorization-evidence-missing");
        var status = String(authorization, "status");
        if (!StringComparer.OrdinalIgnoreCase.Equals(status, "open")
            && !StringComparer.OrdinalIgnoreCase.Equals(status, "approved")
            && !StringComparer.OrdinalIgnoreCase.Equals(status, "admitted"))
            throw new UnauthorizedAccessException("owner-authorization-not-open");

        var checker = ReadObject(action.CheckerVerdictRef, "checker-evidence-missing");
        if (!StringComparer.Ordinal.Equals(String(checker, "verdict"), "PASS"))
            throw new UnauthorizedAccessException("checker-pass-required");
        if (!Bool(checker, "independent"))
            throw new UnauthorizedAccessException("independent-checker-required");
        var maker = String(checker, "makerIdentity");
        var checkerIdentity = String(checker, "checkerIdentity");
        if (!string.IsNullOrWhiteSpace(maker) && !string.IsNullOrWhiteSpace(checkerIdentity)
            && StringComparer.Ordinal.Equals(maker, checkerIdentity))
            throw new UnauthorizedAccessException("checker-maker-must-differ");
        if (!string.IsNullOrWhiteSpace(expectedCandidateDigest))
        {
            var checkedDigest = String(checker, "candidateDigest");
            if (string.IsNullOrWhiteSpace(checkedDigest)
                || !StringComparer.OrdinalIgnoreCase.Equals(checkedDigest, expectedCandidateDigest))
                throw new UnauthorizedAccessException("checker-candidate-digest-mismatch");
        }

        var admission = ReadObject(action.AdmissionRef, "admission-evidence-missing");
        if (!AdmissionAccepted(admission)) throw new UnauthorizedAccessException("seed-admission-required");

        var provenance = Path.GetFullPath(action.ProvenanceRef);
        if (!File.Exists(provenance) || new FileInfo(provenance).Length == 0)
            throw new UnauthorizedAccessException("provenance-evidence-missing");
    }

    private static bool TryValidateNormalGrantBoundary(string path)
    {
        try
        {
            var value = ReadObject(path, "grant-verification-evidence-missing");
            // Only shape validation here; the service verifies this reference against live signed state.
            return !string.IsNullOrWhiteSpace(String(value, "grantId"))
                && value.EnumerateObject().Any(x => x.Name.Equals("keyReference", StringComparison.OrdinalIgnoreCase)
                    && x.Value.ValueKind == JsonValueKind.Object);
        }
        catch (UnauthorizedAccessException) { return false; }
    }

    private static JsonElement ReadObject(string path, string error)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path) || !File.Exists(path))
            throw new UnauthorizedAccessException(error);
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        if (doc.RootElement.ValueKind != JsonValueKind.Object) throw new UnauthorizedAccessException(error);
        return doc.RootElement.Clone();
    }

    private static bool AdmissionAccepted(JsonElement value)
    {
        if (Bool(value, "Admitted") || Bool(value, "admitted")) return true;
        var status = String(value, "status");
        return status is not null && (status.Equals("accepted", StringComparison.OrdinalIgnoreCase)
            || status.Equals("admitted", StringComparison.OrdinalIgnoreCase));
    }

    private static bool Verified(JsonElement value)
    {
        if (Bool(value, "verified") || Bool(value, "Verified")) return true;
        var ok = Bool(value, "ok") || Bool(value, "Ok");
        var status = String(value, "status") ?? String(value, "Status");
        return ok && status is not null && status.Contains("verif", StringComparison.OrdinalIgnoreCase);
    }

    private static string? String(JsonElement value, string name)
    {
        foreach (var property in value.EnumerateObject())
            if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                && property.Value.ValueKind == JsonValueKind.String) return property.Value.GetString();
        return null;
    }

    private static bool Bool(JsonElement value, string name)
    {
        foreach (var property in value.EnumerateObject())
            if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                && property.Value.ValueKind is JsonValueKind.True or JsonValueKind.False) return property.Value.GetBoolean();
        return false;
    }

    private static int? Int(JsonElement value, string name)
    {
        foreach (var property in value.EnumerateObject())
            if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                && property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt32(out var number)) return number;
        return null;
    }

    private static void Require(string value, string error)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new UnauthorizedAccessException(error);
    }
}
