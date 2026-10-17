using Scalar.AspNetCore;
using Microsoft.Extensions.Hosting;
using ProjectName.Harness;
using ProjectName.OrchestrationApi;
using ProjectName.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddProjectNameServiceDefaults();
builder.Services.AddProjectNameAgentSystem();
if (builder.Configuration.GetValue<bool>("AgentSystem:ProductionHost"))
{
    if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PMCRO_CONTROL_PLANE_TRUST_FILE")))
        builder.Services.AddSingleton<IFrameworkActivation, DisabledProductionActivation>();
    else
    {
        var trust = ControlPlaneOperatorTrust.FromEnvironment();
        trust.ValidatePaths(trust.Paths);
        var p = trust.Paths;
        builder.Services.AddProjectNameProductionActivation(p.ActivationRoot,
            new WindowsDpapiSecretProvider(p.ProtectedKeyRoot, p.CanonicalRoot),
            new FileCapabilityRegistrationStore(p.RegistrationRoot), new FileGrantStore(p.GrantRoot),
            new FileRevocationStore(p.RevocationRoot), "windows-dpapi", p.ProtectedKeyRoot);
    }
}
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapProjectNameServiceDefaults();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/api-reference", options =>
    {
        options.WithTitle("ProjectName AgentSystem API");
    });
}
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
app.MapGet("/csuite/readiness", (IFrameworkActivation activation) =>
    activation.IsActivated("csuite.executive-council")
        ? Results.Ok(new { status = "activated", liveModelVerified = false })
        : Results.Json(new { status = "blocked", reason = "verified-csuite-activation-required" }, statusCode: 503));
app.MapPmcroEndpoints();
if (builder.Configuration.GetValue<bool>("AgentSystem:ProductionHost")) app.MapCouncil();
app.Run();
