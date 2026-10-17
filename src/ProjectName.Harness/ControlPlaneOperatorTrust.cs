namespace ProjectName.Harness;

using System.Security.Cryptography;
using System.Text.Json;
using ProjectName.Contracts;

// Deployment input captured outside request JSON. Never populate this from a model or HTTP request.
public sealed record ControlPlaneOperatorTrust(ControlPlaneStatePaths Paths, string OwnerPrincipal,
    string? BootstrapAuthorizationSha256 = null, string? BootstrapCheckerSha256 = null)
{
    public static ControlPlaneOperatorTrust FromEnvironment()
    {
        var trustFile = Environment.GetEnvironmentVariable("PMCRO_CONTROL_PLANE_TRUST_FILE")
            ?? throw new UnauthorizedAccessException("operator-control-plane-trust-file-required");
        if (!Path.IsPathFullyQualified(trustFile) || !File.Exists(trustFile))
            throw new UnauthorizedAccessException("operator-control-plane-trust-file-invalid");
        for (var path = Path.GetFullPath(trustFile); !string.IsNullOrEmpty(path); path = Path.GetDirectoryName(path))
            if ((File.Exists(path) || Directory.Exists(path)) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                throw new UnauthorizedAccessException("operator-trust-reparse-point-rejected");
        return JsonSerializer.Deserialize<ControlPlaneOperatorTrust>(
            File.ReadAllText(Path.GetFullPath(trustFile)),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new UnauthorizedAccessException("operator-control-plane-trust-invalid");
    }

    public void ValidatePaths(ControlPlaneStatePaths request)
    {
        foreach (var (actual, expected) in new[]
        {
            (request.CanonicalRoot, Paths.CanonicalRoot), (request.RegistrationRoot, Paths.RegistrationRoot),
            (request.GrantRoot, Paths.GrantRoot), (request.RevocationRoot, Paths.RevocationRoot),
            (request.ActivationRoot, Paths.ActivationRoot), (request.ProtectedKeyRoot, Paths.ProtectedKeyRoot)
        })
        {
            if (!Path.IsPathFullyQualified(actual) || !Path.IsPathFullyQualified(expected)
                || !Path.GetFullPath(actual).Equals(Path.GetFullPath(expected), StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("operator-state-path-mismatch");
            for (var path = Path.GetFullPath(actual); !string.IsNullOrEmpty(path); path = Path.GetDirectoryName(path))
                if ((File.Exists(path) || Directory.Exists(path)) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                    throw new UnauthorizedAccessException("state-reparse-point-rejected");
        }
    }

    public void ValidateBootstrap(LifecycleActionRequest action, string principal)
    {
        if (string.IsNullOrWhiteSpace(OwnerPrincipal) || principal != OwnerPrincipal)
            throw new UnauthorizedAccessException("operator-owner-mismatch");
        Pin(action.AuthorizationRef, BootstrapAuthorizationSha256);
        Pin(action.CheckerVerdictRef, BootstrapCheckerSha256);
    }

    private static void Pin(string path, string? expected)
    {
        if (expected?.Length != 64 || !File.Exists(path)
            || !Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).Equals(expected, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("operator-bootstrap-evidence-pin-mismatch");
    }
}
