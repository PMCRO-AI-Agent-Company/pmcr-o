namespace ProjectName.Core;

using System.Text.RegularExpressions;
using ProjectName.Contracts;

public interface IPmcroIngressProtocol
{
    object ParseOrReject(string message);
    PmcroFirstResponse CreateFirstResponse(string externalRequest, bool trailActive);
    PmcroInitAcknowledgement Initialize(PmcroInitRequest request);
}

public sealed class PmcroIngressProtocol : IPmcroIngressProtocol
{
    private static readonly Regex RoutePattern = new(
        "^(?:@(?<agent>[a-z0-9-]+)\\s+)?/pmcro:(?<command>init|seed|reject|resume|defer|orchestrate|plan|make|check|reflect|create|update|remove|reconcile)(?:\\s+\\\"(?<intent>.*)\\\")?\\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private readonly HashSet<string> _initializedTrails = new(StringComparer.Ordinal);

    public object ParseOrReject(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return Reject(message ?? string.Empty, "Empty message");

        var match = RoutePattern.Match(message.Trim());
        if (!match.Success)
            return Reject(message, "Operational PMCR-O text must use a canonical /pmcro:<command> route or a validated machine envelope.");

        var agent = match.Groups["agent"].Success
            ? match.Groups["agent"].Value.ToLowerInvariant()
            : "orchestrator";
        var command = match.Groups["command"].Value.ToLowerInvariant();
        var intent = match.Groups["intent"].Success ? match.Groups["intent"].Value : null;

        if (command is "init" or "seed" && string.IsNullOrWhiteSpace(intent))
            return Reject(message, $"/pmcro:{command} requires a quoted intent.");

        return new ParsedPmcroRoute(agent, command, intent, message);
    }

    public PmcroFirstResponse CreateFirstResponse(string externalRequest, bool trailActive)
    {
        if (string.IsNullOrWhiteSpace(externalRequest))
            throw new ArgumentException("external request required", nameof(externalRequest));

        var escaped = externalRequest.Replace("\\", "\\\\").Replace("\"", "\\\"");
        var command = trailActive ? "seed" : "init";
        var route = $"@orchestrator /pmcro:{command} \"{escaped}\"";

        return new PmcroFirstResponse(
            Route: route,
            Command: command,
            Intent: externalRequest,
            AgentRef: "pmcro-orchestrator",
            Status: trailActive ? "SEED" : "INITIALIZE",
            AdmittedByResponse: false);
    }

    public PmcroInitAcknowledgement Initialize(PmcroInitRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Intent))
            throw new ArgumentException("intent required", nameof(request));
        if (string.IsNullOrWhiteSpace(request.AgentRef))
            throw new ArgumentException("agentRef required", nameof(request));

        var projectId = string.IsNullOrWhiteSpace(request.ProjectId) ? "ProjectName" : request.ProjectId;
        var trailId = string.IsNullOrWhiteSpace(request.TrailRef)
            ? $"trail-{Guid.NewGuid():N}"
            : request.TrailRef;

        if (!_initializedTrails.Add(trailId))
            throw new InvalidOperationException(
                $"Trail '{trailId}' is already initialized. Submit later intent with /pmcro:seed instead.");

        const int cycleNumber = 1;
        var seedId = $"{trailId}-S001";
        var cycleId = $"{trailId}-C001";

        return new PmcroInitAcknowledgement(
            Route: "@orchestrator /pmcro:init",
            Status: "Cycle 1 initialized",
            ProjectId: projectId,
            TrailId: trailId,
            SeedId: seedId,
            CycleId: cycleId,
            CycleNumber: cycleNumber,
            Intent: request.Intent,
            NextRoute: "@orchestrator /pmcro:orchestrate");
    }

    private static PmcroIngressRejection Reject(string received, string reason)
    {
        var escaped = received.Replace("\\", "\\\\").Replace("\"", "\\\"");
        return new PmcroIngressRejection(
            Status: "REJECTED",
            Reason: reason,
            Received: received,
            CorrectedRoute: $"@orchestrator /pmcro:seed \"{escaped}\"",
            Guidance: "Guidance: use @orchestrator /pmcro:init for a new Trail, or @orchestrator /pmcro:seed for later intent on an active Trail. A Seed still requires an Orchestrator admission decision.");
    }
}
