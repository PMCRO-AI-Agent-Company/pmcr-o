namespace ProjectName.Runtime.Maf;

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using ProjectName.Contracts;

public sealed record MafExecutiveCouncilProjection(
    Workflow Workflow,
    IReadOnlyList<string> OfficeAgentIds,
    string Mermaid,
    string Dot,
    string Pattern = "concurrent");

public sealed class MafExecutiveCouncilCompiler(
    IChatClient chatClient,
    IMafGovernanceBoundary governance)
{
    public MafExecutiveCouncilProjection Compile(
        ExecutiveCouncilRequest request,
        IReadOnlyList<ExecutiveOfficeDefinition> offices,
        IReadOnlySet<string> effectiveGrant)
    {
        var agents = CreateSelectedAgents(request, offices, effectiveGrant);
        var workflow = AgentWorkflowBuilder.BuildConcurrent("PMCR-O Executive Council", agents, aggregator: null);
        return Project(workflow, agents, "concurrent");
    }

    public MafExecutiveCouncilProjection Compile(
        ExecutiveCouncilRequest request,
        ExecutiveOrganizationManifest organization,
        IReadOnlySet<string> effectiveGrant) =>
        Compile(request with { RequestedOfficeIds = [] }, organization.ResolveOffices(request.RequestedOfficeIds), effectiveGrant);

    public MafExecutiveCouncilProjection CompileBoundedGroupChat(
        ExecutiveCouncilRequest request,
        IReadOnlyList<ExecutiveOfficeDefinition> offices,
        IReadOnlySet<string> effectiveGrant)
    {
        if (request.MaxDebateRounds <= 0)
            throw new InvalidOperationException("executive-debate-budget-required");

        var agents = CreateSelectedAgents(request, offices, effectiveGrant);
        var maxMessages = Math.Max(1, request.MaxDebateRounds) * agents.Length;
        var workflow = AgentWorkflowBuilder
            .CreateGroupChatBuilderWith(participants => new RoundRobinGroupChatManager(
                participants,
                (_, history, _) => ValueTask.FromResult(history.Count() >= maxMessages)))
            .AddParticipants(agents)
            .Build();
        return Project(workflow, agents, "bounded-group-chat");
    }

    public MafExecutiveCouncilProjection CompileHandoff(
        ExecutiveCouncilRequest request,
        IReadOnlyList<ExecutiveOfficeDefinition> offices,
        IReadOnlySet<string> effectiveGrant,
        string? initialOfficeId = null)
    {
        var agents = CreateSelectedAgents(request, offices, effectiveGrant);
        var initial = string.IsNullOrWhiteSpace(initialOfficeId)
            ? agents[0]
            : agents.FirstOrDefault(a => StringComparer.Ordinal.Equals(a.Id, "executive-" + initialOfficeId))
              ?? throw new InvalidOperationException("initial-executive-office-not-found");

        var builder = AgentWorkflowBuilder.CreateHandoffBuilderWith(initial);
        builder.AddParticipants(agents.Where(a => !ReferenceEquals(a, initial)));
        var workflow = builder.Build();
        return Project(workflow, agents, "handoff");
    }

    public static ExecutiveCouncilNextMode SelectNextMode(
        ExecutiveCouncilRequest request,
        ExecutiveRecommendationBundle bundle)
    {
        if (request.RequireOwnerApproval || bundle.RequiresOwnerApproval)
            return ExecutiveCouncilNextMode.HumanInTheLoop;
        if (bundle.MaterialDisagreements.Count > 0 && request.MaxDebateRounds > 0)
            return ExecutiveCouncilNextMode.BoundedGroupChat;
        if (bundle.DelegationProposals.Count > 0)
            return ExecutiveCouncilNextMode.Handoff;
        return ExecutiveCouncilNextMode.Complete;
    }

    public static string BuildOfficeInstructions(ExecutiveOfficeDefinition office) =>
        office.VisibleIdentity + "\n" +
        "Read the existing council dialogue before responding so your turn is causally aware. " +
        "Every visible turn MUST begin exactly with the identity statement above; do not replace the full office name with an abbreviation.\n" +
        "Stay inside this executive scope: " + office.Scope + "\n" + office.Instructions + "\n" +
        "After the identity line, state DEPENDS ON: <request, prior turn, speaker, or claim>. " +
        "Then state RELATION: ACCEPTS, CHALLENGES, EXTENDS, REQUESTS, or VERIFIES. " +
        "Preserve evidence gaps, risks, dissent, assumptions, and delegation proposals. " +
        "End with NEXT: <full role name> and NEXT DEPENDENCY: <what that role must answer or verify>, " +
        "or end with CHAIN CLOSED when no dependency remains. " +
        "Never claim execution authority, never self-approve, and never collapse disagreement into false consensus.";

    private AIAgent[] CreateSelectedAgents(
        ExecutiveCouncilRequest request,
        IReadOnlyList<ExecutiveOfficeDefinition> offices,
        IReadOnlySet<string> effectiveGrant)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (offices.Count == 0) throw new ArgumentException("executive-offices-required", nameof(offices));
        if (effectiveGrant.Count == 0) throw new UnauthorizedAccessException("effective-grant-required");

        var selected = request.RequestedOfficeIds.Count == 0
            ? offices
            : offices.Where(o => request.RequestedOfficeIds.Contains(o.OfficeId, StringComparer.Ordinal)).ToArray();
        if (selected.Count == 0) throw new InvalidOperationException("requested-executive-offices-not-found");
        return selected.Select(office => CreateOfficeAgent(office, effectiveGrant)).ToArray();
    }

    private MafExecutiveCouncilProjection Project(Workflow workflow, IReadOnlyList<AIAgent> agents, string pattern) =>
        new(workflow, agents.Select(a => a.Id).ToArray(),
            WorkflowVisualizer.ToMermaidString(workflow), WorkflowVisualizer.ToDotString(workflow), pattern);

    private AIAgent CreateOfficeAgent(ExecutiveOfficeDefinition office, IReadOnlySet<string> effectiveGrant)
    {
        AIAgent agent = chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Id = "executive-" + office.OfficeId,
            Name = office.DisplayName,
            ChatOptions = new ChatOptions
            {
                Instructions = BuildOfficeInstructions(office),
                MaxOutputTokens = office.MaxOutputTokens,
                Tools = [],
            },
        });
        return governance.Govern(agent, agent.Id, effectiveGrant, delegated: true);
    }
}
