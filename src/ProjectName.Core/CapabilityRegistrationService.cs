using ProjectName.Contracts;

namespace ProjectName.Core;

public sealed class CapabilityRegistrationService
{
    public bool Validate(CapabilityRegistration registration)
    {
        return registration.Status == "registered"
            && registration.RegistrationId.Length > 0
            && registration.CapabilityId.Length > 0
            && IsSha256(registration.CandidateDigest)
            && IsSha256(registration.FileManifestDigest)
            && registration.CheckerVerdictRef.Length > 0
            && registration.ProvenanceRef.Length > 0;
    }

    private static bool IsSha256(string value) =>
        value.Length == 64 && value.All(Uri.IsHexDigit);
}
