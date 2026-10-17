using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OllamaSharp;
using ProjectName.Contracts;
using ProjectName.Harness;
using ProjectName.Infrastructure;
using ProjectName.Runtime.Maf;

namespace ProjectName.OrchestrationApi;

public sealed record CouncilQuestion(string Question);

public static class CouncilEndpoints
{
    public static void MapCouncil(this WebApplication app)
    {
        // Capture deployment authority once, outside HTTP/model input.
        var rawTrust = Environment.GetEnvironmentVariable("PMCRO_CONTROL_PLANE_TRUST_FILE");
        var trust = string.IsNullOrWhiteSpace(rawTrust) ? null : ControlPlaneOperatorTrust.FromEnvironment();
        trust?.ValidatePaths(trust.Paths);
        var connection = app.Configuration.GetConnectionString("agentsystem-ollama");
        var endpoint = connection?.StartsWith("Endpoint=", StringComparison.OrdinalIgnoreCase) == true ? connection[9..].Split(';')[0] : connection;
        var model = app.Configuration["AgentSystem:Model"] ?? "qwen3:8b";
        var serial = new SemaphoreSlim(1, 1);
        app.MapPost("/csuite/council", async (CouncilQuestion input, HttpContext context, IFrameworkActivation gate) =>
        {
            if (context.Connection.RemoteIpAddress is not { } remote || !IPAddress.IsLoopback(remote)) return Results.StatusCode(403);
            if (trust is null || endpoint is null || !gate.IsActivated("csuite.executive-council"))
                return Results.Json(new { status = "blocked", reason = "checked-activation-and-model-required" }, statusCode: 503);
            if (string.IsNullOrWhiteSpace(input.Question) || input.Question.Length > 4096) return Results.BadRequest(new { error = "question-must-be-1-to-4096-characters" });
            if (!await serial.WaitAsync(0, context.RequestAborted)) return Results.StatusCode(429);
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
                timeout.CancelAfter(TimeSpan.FromMinutes(2));
                var active = new FileActivationStore(trust.Paths.ActivationRoot).Get("csuite.executive-council")!;
                var authority = active.EffectiveGrant.ToHashSet(StringComparer.Ordinal);
                if (!authority.Contains("capability.invoke") || !authority.Contains("csuite.executive-council")) return Results.StatusCode(403);
                using var http = new HttpClient { BaseAddress = new Uri(endpoint), Timeout = TimeSpan.FromMinutes(2) };
                using var chat = new OllamaApiClient(http) { SelectedModel = model };
                var boundary = new ActiveCouncilBoundary(gate);
                var compiler = new MafExecutiveCouncilCompiler(chat, boundary);
                var offices = new ExecutiveOfficeDefinition[]
                {
                    new("ceo", ExecutiveOfficeNames.ChiefExecutiveOfficer, "strategy and coordination", "Preserve dissent and uncertainty; make no execution decisions.", ["strategy"], MaxOutputTokens: 1024),
                    new("cto", ExecutiveOfficeNames.ChiefTechnologyOfficer, "technical feasibility", "Challenge unsupported technical claims.", ["technology"], MaxOutputTokens: 1024),
                    new("cfo", ExecutiveOfficeNames.ChiefFinancialOfficer, "cost and financial assumptions", "Label estimates and evidence gaps.", ["finance"], MaxOutputTokens: 1024)
                };
                var session = Guid.NewGuid().ToString("D");
                var request = new ExecutiveCouncilRequest(session, session, input.Question, [], [], MaxDebateRounds: 1);
                var projection = compiler.CompileBoundedGroupChat(request, offices, authority);
                var trailRoot = Path.Combine(trust.Paths.CanonicalRoot, "local", ".pmcro", "trails", session);
                using var runtime = new MafExecutiveWorkflowRuntime(Path.Combine(trailRoot, "checkpoints"));
                var run = await runtime.RunAsync(projection.Workflow, new List<ChatMessage> { new(ChatRole.User, input.Question) }, session, timeout.Token);
                var replies = run.OutgoingEvents.OfType<AgentResponseEvent>().Select(e => e.Response.Text).ToArray();
                if (!gate.IsActivated("csuite.executive-council")) throw new UnauthorizedAccessException("activation-changed-during-run");
                if (replies.Length == 0) throw new InvalidOperationException("council-produced-no-responses");
                var result = new { sessionId = session, model, replies, authoritative = false, grantsAuthority = false };
                await using var evidence = new FileStream(Path.Combine(trailRoot, "council-result.json"), FileMode.CreateNew, FileAccess.Write, FileShare.None);
                await JsonSerializer.SerializeAsync(evidence, result, cancellationToken: timeout.Token);
                await evidence.FlushAsync(timeout.Token);
                return Results.Ok(result);
            }
            catch (OperationCanceledException) { return Results.Json(new { error = "council-time-budget-exceeded" }, statusCode: 504); }
            catch (UnauthorizedAccessException) { return Results.StatusCode(403); }
            catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException) { return Results.Json(new { error = "council-execution-failed" }, statusCode: 502); }
            finally { serial.Release(); }
        });
    }

    // Tools are absent from compiler-created agents. This boundary revalidates signed activation at every model run.
    private sealed class ActiveCouncilBoundary(IFrameworkActivation gate) : IMafGovernanceBoundary
    {
        private void Admit(IReadOnlySet<string> grant)
        {
            if (!grant.Contains("capability.invoke") || !grant.Contains("csuite.executive-council") || !gate.IsActivated("csuite.executive-council"))
                throw new UnauthorizedAccessException("active-council-grant-required");
        }
        public AIAgent Govern(AIAgent agent, string agentId, IReadOnlySet<string> effectiveGrant, bool delegated)
        {
            Admit(effectiveGrant);
            return agent.AsBuilder().Use(async (messages, session, options, inner, ct) =>
            {
                Admit(effectiveGrant);
                return await inner.RunAsync(messages, session, options, ct);
            }, (messages, session, options, inner, ct) => Stream(messages, session, options, inner, effectiveGrant, ct)).Build();
        }
        private async IAsyncEnumerable<AgentResponseUpdate> Stream(IEnumerable<ChatMessage> messages, AgentSession? session,
            AgentRunOptions? options, AIAgent inner, IReadOnlySet<string> grant, [EnumeratorCancellation] CancellationToken ct)
        {
            Admit(grant);
            await foreach (var update in inner.RunStreamingAsync(messages, session, options, ct))
            {
                Admit(grant);
                yield return update;
            }
        }
    }
}
