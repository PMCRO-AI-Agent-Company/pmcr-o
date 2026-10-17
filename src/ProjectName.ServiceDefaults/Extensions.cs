using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

public static class ProjectNameServiceDefaults
{
    private const string HealthPath = "/health";
    private const string AlivePath = "/alive";

    public static TBuilder AddProjectNameServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(o => { o.IncludeFormattedMessage = true; o.IncludeScopes = true; });
        builder.Services.AddOpenTelemetry()
            .WithMetrics(m => m.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddRuntimeInstrumentation())
            .WithTracing(t => t.AddSource(builder.Environment.ApplicationName)
                .AddAspNetCoreInstrumentation(o => o.Filter = c => !c.Request.Path.StartsWithSegments(HealthPath) && !c.Request.Path.StartsWithSegments(AlivePath))
                .AddHttpClientInstrumentation());
        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        builder.Services.AddHealthChecks().AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });
        return builder;
    }

    public static WebApplication MapProjectNameServiceDefaults(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return app;
        app.MapHealthChecks(HealthPath);
        app.MapHealthChecks(AlivePath, new HealthCheckOptions { Predicate = r => r.Tags.Contains("live") });
        return app;
    }
}
