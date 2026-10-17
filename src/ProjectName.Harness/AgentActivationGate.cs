namespace ProjectName.Harness;

using System.Security.Cryptography;
using System.Text;
using ProjectName.Contracts;
using ProjectName.Core;

public sealed class AgentActivationGate(
    ProductionGrantVerifier grantVerifier,
    ICapabilityRegistrationStore registrations,
    IRevocationStore revocations,
    IActivationStore activations,
    string keyProvider,
    string keyLocationRef) : IFrameworkActivation
{
    public Task<AgentExecutionGrant> ActivateAsync(string frameworkId, AgentActivationReceipt receipt, AgentExecutionGrant ceiling, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(frameworkId);
        if (receipt.Verdict != "PASS") throw new UnauthorizedAccessException("activation-requires-pass");
        if (string.IsNullOrWhiteSpace(receipt.MakerIdentity) || string.IsNullOrWhiteSpace(receipt.CheckerIdentity) || StringComparer.Ordinal.Equals(receipt.MakerIdentity, receipt.CheckerIdentity))
            throw new UnauthorizedAccessException("independent-checker-required");
        if (receipt.CheckerIdentity is not ("pmcro.checker" or "pmcro-checker" or "checker"))
            throw new UnauthorizedAccessException("checker-identity-not-accepted");

        var grant = receipt.Grant;
        if (string.IsNullOrWhiteSpace(grant.GrantId) || string.IsNullOrWhiteSpace(grant.RegistrationRef) || string.IsNullOrWhiteSpace(grant.CandidateDigest) || string.IsNullOrWhiteSpace(grant.KeyId) || grant.KeyGeneration is null)
            throw new UnauthorizedAccessException("signed-grant-binding-incomplete");
        if (!StringComparer.OrdinalIgnoreCase.Equals(receipt.CandidateDigest, grant.CandidateDigest))
            throw new UnauthorizedAccessException("candidate-digest-mismatch");

        var registration = registrations.Get(grant.RegistrationRef) ?? throw new UnauthorizedAccessException("registration-not-found");
        if (!StringComparer.Ordinal.Equals(registration.Status, "registered") || !StringComparer.Ordinal.Equals(registration.CapabilityId, grant.CapabilityId) || !StringComparer.OrdinalIgnoreCase.Equals(registration.CandidateDigest, grant.CandidateDigest))
            throw new UnauthorizedAccessException("registration-binding-mismatch");
        if (!StringComparer.Ordinal.Equals(registration.CheckerVerdictRef, grant.CheckerVerdictRef) || !StringComparer.Ordinal.Equals(registration.ProvenanceRef, grant.ProvenanceRef))
            throw new UnauthorizedAccessException("provenance-binding-mismatch");
        if (!StringComparer.OrdinalIgnoreCase.Equals(registration.FileManifestDigest, ComputeFileManifestDigest(receipt.Files)))
            throw new UnauthorizedAccessException("receipt-file-manifest-mismatch");
        if (!VerifyInstalledFiles(receipt.Files)) throw new UnauthorizedAccessException("installed-candidate-bytes-mismatch");

        var keyRef = new SecretKeyReference(grant.KeyId, grant.KeyGeneration.Value, keyProvider, keyLocationRef);
        if (!grantVerifier.Verify(grant, keyRef)) throw new UnauthorizedAccessException("grant-verification-failed");
        if (revocations.IsRevoked("activation", frameworkId)) throw new UnauthorizedAccessException("activation-revoked");

        var signedEffective = grant.EffectiveGrant ?? grant.DelegatedGrant;
        var ceilingEffective = ceiling.EffectiveGrant ?? ceiling.DelegatedGrant;
        var effective = signedEffective.Intersect(ceilingEffective, StringComparer.Ordinal)
            .Intersect(grant.HostMaximum, StringComparer.Ordinal).Intersect(grant.EnvironmentMaximum, StringComparer.Ordinal)
            .Intersect(ceiling.HostMaximum, StringComparer.Ordinal).Intersect(ceiling.EnvironmentMaximum, StringComparer.Ordinal)
            .Except(grant.Denies, StringComparer.Ordinal).Except(ceiling.Denies, StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal).ToArray();
        if (effective.Except(signedEffective, StringComparer.Ordinal).Any()) throw new UnauthorizedAccessException("authority-widening-rejected");

        var narrowed = grant with
        {
            DelegatedGrant = effective,
            EffectiveGrant = effective,
            HostMaximum = grant.HostMaximum.Intersect(ceiling.HostMaximum, StringComparer.Ordinal).ToArray(),
            EnvironmentMaximum = grant.EnvironmentMaximum.Intersect(ceiling.EnvironmentMaximum, StringComparer.Ordinal).ToArray(),
            Denies = grant.Denies.Union(ceiling.Denies, StringComparer.Ordinal).Distinct(StringComparer.Ordinal).ToArray(),
            Source = "activation-gate:" + frameworkId
        };

        activations.Save(new ActivationRecord(frameworkId, "active", ComputeReceiptDigest(receipt), grant.GrantId, grant.RegistrationRef,
            grant.CandidateDigest, effective, receipt.CheckerIdentity, grant.ProvenanceRef ?? "", grant.KeyId, grant.KeyGeneration.Value, DateTimeOffset.UtcNow, receipt.Files, receipt.MakerIdentity));
        return Task.FromResult(narrowed);
    }

    public bool IsActivated(string frameworkId)
    {
        var record = activations.Get(frameworkId);
        if (record is null || record.Status != "active" || revocations.IsRevoked("activation", frameworkId)) return false;
        var registration = registrations.Get(record.RegistrationRef);
        if (registration is null || registration.Status != "registered" || !StringComparer.OrdinalIgnoreCase.Equals(registration.CandidateDigest, record.CandidateDigest)) return false;
        if (record.Files is null || !VerifyInstalledFiles(record.Files)
            || ComputeFileManifestDigest(record.Files) != registration.FileManifestDigest) return false;
        var grant = grantVerifier.GetVerified(record.GrantId, new SecretKeyReference(record.KeyId, record.KeyGeneration, keyProvider, keyLocationRef));
        if (grant is null || grant.RegistrationRef != record.RegistrationRef || grant.CandidateDigest != record.CandidateDigest
            || grant.CapabilityId != registration.CapabilityId || grant.ProvenanceRef != record.ProvenanceRef
            || grant.CheckerVerdictRef != registration.CheckerVerdictRef
            || record.EffectiveGrant.Except(grant.EffectiveGrant ?? [], StringComparer.Ordinal).Any()) return false;
        return !string.IsNullOrWhiteSpace(record.MakerIdentity) && record.ReceiptDigest == ComputeReceiptDigest(new AgentActivationReceipt("PASS", record.MakerIdentity, record.CheckerIdentity,
            record.CandidateDigest, record.Files, grant));
    }

    public static bool VerifyInstalledFiles(IReadOnlyDictionary<string, string> files)
    {
        if (files.Count == 0) return false;
        try
        {
            foreach (var (path, digest) in files)
            {
                if (!Path.IsPathFullyQualified(path) || !File.Exists(path)) return false;
                for (var part = Path.GetFullPath(path); !string.IsNullOrEmpty(part); part = Path.GetDirectoryName(part))
                    if ((File.GetAttributes(part) & FileAttributes.ReparsePoint) != 0) return false;
                using var stream = File.OpenRead(path);
                if (!Convert.ToHexString(SHA256.HashData(stream)).Equals(digest, StringComparison.OrdinalIgnoreCase)) return false;
            }
            return true;
        }
        catch (IOException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
    }

    public static string ComputeFileManifestDigest(IReadOnlyDictionary<string, string> files) => Sha256(string.Join("\n", files.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => x.Key + "=" + x.Value.ToLowerInvariant())));

    public static string ComputeReceiptDigest(AgentActivationReceipt receipt) => Sha256(string.Join("|", new[]
    {
        receipt.Verdict, receipt.MakerIdentity, receipt.CheckerIdentity, receipt.CandidateDigest,
        ComputeFileManifestDigest(receipt.Files), GrantDerivationService.CanonicalPayload(receipt.Grant), receipt.Grant.IssuerProof ?? ""
    }));

    private static string Sha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
