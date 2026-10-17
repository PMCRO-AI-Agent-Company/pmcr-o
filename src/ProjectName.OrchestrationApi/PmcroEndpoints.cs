namespace ProjectName.OrchestrationApi;

using ProjectName.Application.Abstractions;
using ProjectName.Contracts;
using ProjectName.Core;
using ProjectName.Harness;

public static class PmcroEndpoints
{
    public static IEndpointRouteBuilder MapPmcroEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/pmcro/ingress", async (HttpRequest request, IPmcroIngressProtocol ingress) =>
        {
            using var reader = new StreamReader(request.Body);
            var message = await reader.ReadToEndAsync();
            var parsed = ingress.ParseOrReject(message);
            return parsed is PmcroIngressRejection rejection
                ? Results.BadRequest(rejection)
                : Results.Ok(parsed);
        })
        .WithName("PmcroIngress")
        .WithTags("pmcro");

        app.MapPost("/api/pmcro/init", (
            PmcroInitRequest body,
            IPmcroIngressProtocol ingress) =>
        {
            try
            {
                return Results.Ok(ingress.Initialize(body));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new
                {
                    error = ex.Message,
                    correctedRoute = $"@orchestrator /pmcro:seed \"{body.Intent.Replace("\"", "\\\"")}\""
                });
            }
        })
        .WithName("PmcroInit")
        .WithTags("pmcro");

        app.MapPost("/api/pmcro/request", async (
            PmcroRequestEnvelope body,
            IPhaseRouter router,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(body.Intent))
                return Results.BadRequest(new { error = "intent is required" });
            if (string.IsNullOrWhiteSpace(body.Command))
                return Results.BadRequest(new { error = "command is required" });
            if (string.IsNullOrWhiteSpace(body.AgentRef))
                return Results.BadRequest(new { error = "agentRef is required" });

            try
            {
                var ack = await router.DispatchAsync(body.ToContract(), ct);
                return Results.Ok(ack);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("PmcroRequest")
        .WithTags("pmcro");

        app.MapGet("/api/pmcro/commands", (IPmcroOperations operations) => Results.Ok(operations.GetCommands()))
        .WithName("PmcroCommands")
        .WithTags("pmcro");

        return app;
    }
}
