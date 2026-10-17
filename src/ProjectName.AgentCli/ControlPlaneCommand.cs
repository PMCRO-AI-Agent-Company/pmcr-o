namespace ProjectName.AgentCli;

using System.Text.Json;
using ProjectName.Contracts;
using ProjectName.Core;
using ProjectName.Harness;
using ProjectName.Infrastructure;

public static class ControlPlaneCommand
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static async Task<int> RunAsync(string[] args, CancellationToken ct = default)
    {
        string? requestPath = null;
        var mode = "simulate";
        var ownerBootstrap = false;
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "--request" && i + 1 < args.Length) requestPath = args[++i];
            else if (args[i] == "--owner-bootstrap") ownerBootstrap = true;
            else if (args[i] == "--mode" && i + 1 < args.Length) mode = args[++i];
            else if (args[i] is "--help" or "-h") { PrintHelp(); return 0; }
            else throw new ArgumentException("unknown-control-plane-option:" + args[i]);
        }

        if (string.IsNullOrWhiteSpace(requestPath)) throw new ArgumentException("request-required");
        if (mode is not ("preflight" or "simulate" or "execute" or "check")) throw new ArgumentException("invalid-mode");

        var fullRequestPath = Path.GetFullPath(requestPath);
        if (!File.Exists(fullRequestPath)) throw new FileNotFoundException("request-file-missing", fullRequestPath);
        if (ownerBootstrap) return RunOwnerBootstrap(fullRequestPath, mode);
        var request = JsonSerializer.Deserialize<ControlPlaneCommissioningRequest>(File.ReadAllText(fullRequestPath), JsonOptions)
            ?? throw new InvalidOperationException("request-invalid");
        var operatorTrust = ControlPlaneOperatorTrust.FromEnvironment();
        operatorTrust.ValidatePaths(request.Paths);
        var roots = operatorTrust.Paths;
        request = request with { Paths = roots };
        if (request.KeyReference is not null)
        {
            if (!StringComparer.OrdinalIgnoreCase.Equals(request.KeyReference.Provider, "windows-dpapi"))
                throw new InvalidOperationException("production-key-provider-must-be-windows-dpapi");
            if (!Path.GetFullPath(request.KeyReference.LocationRef).Equals(Path.GetFullPath(roots.ProtectedKeyRoot), StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("production-key-location-mismatch");
        }

        ControlPlaneCommissioningService.ValidatePreflight(request);
        var readOnlyOperation = request.Operation is "verify-grant" or LifecycleOperations.InspectState;
        if ((mode == "check" || readOnlyOperation) && MissingRequiredStateRoot(request))
        {
            Write(new ControlPlaneCommissioningResult(false, request.Operation, mode, "state-not-present", request.TrailAction.TrailId));
            return 2;
        }


        var secrets = new WindowsDpapiSecretProvider(roots.ProtectedKeyRoot, roots.CanonicalRoot);
        var registrations = new FileCapabilityRegistrationStore(roots.RegistrationRoot);
        var grants = new FileGrantStore(roots.GrantRoot);
        var revocations = new FileRevocationStore(roots.RevocationRoot);
        var activations = new FileActivationStore(roots.ActivationRoot);
        var issuer = new ProductionGrantIssuer(secrets, registrations, grants, revocations);
        var verifier = new ProductionGrantVerifier(secrets, grants, revocations);
        var keyProvider = request.KeyReference?.Provider ?? "windows-dpapi";
        var keyLocation = request.KeyReference?.LocationRef ?? roots.ProtectedKeyRoot;
        var gate = new AgentActivationGate(verifier, registrations, revocations, activations, keyProvider, keyLocation);
        var service = new ControlPlaneCommissioningService(
            secrets, registrations, grants, revocations, issuer, verifier, gate);

        var result = await service.RunAsync(request, mode, ct);
        Write(result);
        return result.Ok ? 0 : 2;
    }

    private static int RunOwnerBootstrap(string path, string mode)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("windows-owner-bootstrap-required");
        var request = JsonSerializer.Deserialize<FirstOperationalGrantRequest>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidOperationException("bootstrap-request-invalid");
        var operatorTrust = ControlPlaneOperatorTrust.FromEnvironment();
        operatorTrust.ValidatePaths(request.Paths);
        var roots = operatorTrust.Paths;
        request = request with { Paths = roots };
        // All durable production stores are selected by separately established operator trust, never by request JSON.
        var state = Path.Combine(Path.GetFullPath(roots.CanonicalRoot), "global", ".pmcro");
        foreach (var (supplied, expected) in new[]
        {
            (roots.RegistrationRoot, Path.Combine(state, "registry", "records")),
            (roots.GrantRoot, Path.Combine(state, "grants", "records")),
            (roots.RevocationRoot, Path.Combine(state, "revocations")),
            (roots.ActivationRoot, Path.Combine(state, "activations"))
        })
            if (!Path.GetFullPath(supplied).Equals(Path.GetFullPath(expected), StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("bootstrap-state-root-mismatch");
        var secrets = new WindowsDpapiSecretProvider(roots.ProtectedKeyRoot, roots.CanonicalRoot);
        var registrations = new FileCapabilityRegistrationStore(roots.RegistrationRoot);
        var grants = new FileGrantStore(roots.GrantRoot);
        var revocations = new FileRevocationStore(roots.RevocationRoot);
        using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
        var service = new FirstOperationalGrantBootstrap(secrets, registrations, grants,
            new ProductionGrantIssuer(secrets, registrations, grants, revocations),
            new ProductionGrantVerifier(secrets, grants, revocations), identity.User?.Value ?? "", operatorTrust);
        var result = service.Run(request, mode);
        Write(result);
        return result.Ok ? 0 : 2;
    }

    private static bool MissingRequiredStateRoot(ControlPlaneCommissioningRequest request)
    {
        var p = request.Paths;
        if (!Directory.Exists(p.RegistrationRoot) || !Directory.Exists(p.GrantRoot)
            || !Directory.Exists(p.RevocationRoot) || !Directory.Exists(p.ActivationRoot)) return true;
        if (request.KeyReference is not null && !Directory.Exists(p.ProtectedKeyRoot)) return true;
        return false;
    }

    private static void Write(ControlPlaneCommissioningResult result) =>
        Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));

    public static void PrintHelp()
    {
        Console.WriteLine("control-plane --request <request.json> --mode preflight|simulate|execute|check");
        Console.WriteLine("Operations: provision-key-generation, register-capability, issue-minimum-grant, verify-grant, activate-capability, revoke, inspect-state");
        Console.WriteLine("Mutating operations require Trail evidence, owner authorization, independent Checker PASS, provenance, and a verified grant boundary.");
        Console.WriteLine("Secret material is never printed.");
        Console.WriteLine("--owner-bootstrap selects the single-use, owner-authenticated first operational grant path; requires a fresh checked registration and explicit scoped owner authorization.");
    }
}
