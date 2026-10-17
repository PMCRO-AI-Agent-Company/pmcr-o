using ProjectName.Contracts;
using Xunit;

namespace ProjectName.Runtime.Tests;

public class CognitiveTrailTests
{
    [Theory]
    [InlineData("orchestrator", "ORCHESTRATE", true)]
    [InlineData("planner", "PLAN", true)]
    [InlineData("maker", "MAKE", true)]
    [InlineData("checker", "CHECK", true)]
    [InlineData("reflector", "REFLECT", true)]
    [InlineData("planner", "CHECK", false)]
    [InlineData("maker", "PLAN", false)]
    [InlineData("reflector", "ORCHESTRATE", false)]
    public void Phase_owner_mapping_controls_authoritative_commit(string role, string phase, bool expected)
    {
        Assert.Equal(expected, PmcroFrameCatalog.IsAuthoritative(role, phase));
    }

    [Fact]
    public void Dialogue_turn_can_reference_belief_and_evidence_without_becoming_authoritative()
    {
        var turn = new CognitiveDialogueTurn(
            TurnId: "T1",
            SpeakerRole: "planner",
            CognitiveLens: "CHECK",
            AddressedTo: ["checker"],
            Statement: "I expect this acceptance criterion to be independently testable.",
            Question: null,
            Challenge: "Can you prove it from evidence?",
            Response: null,
            EvidenceRefs: ["evidence://candidate"],
            PriorBeliefRefs: ["belief://cycle-1/plan"],
            RevisedBelief: null,
            UnresolvedTensions: ["independent proof pending"],
            SelfReference: "planner observing its own assumption",
            AuthoritativeFrameRef: "checker:CHECK");

        Assert.Equal("checker", turn.AuthoritativeOwner);
        Assert.False(turn.IsAuthoritativeTurn);
        Assert.Single(turn.EvidenceRefs);
        Assert.Single(turn.PriorBeliefRefs);
    }

    [Fact]
    public void Authoritative_owner_turn_is_identified_but_dialogue_object_itself_grants_no_authority()
    {
        var turn = new CognitiveDialogueTurn(
            TurnId: "T2",
            SpeakerRole: "checker",
            CognitiveLens: "CHECK",
            AddressedTo: ["reflector"],
            Statement: "Verified result.",
            Question: null,
            Challenge: null,
            Response: null,
            EvidenceRefs: ["evidence://verified"],
            PriorBeliefRefs: [],
            RevisedBelief: "The implementation passed independent checks.",
            UnresolvedTensions: [],
            SelfReference: null,
            AuthoritativeFrameRef: "checker:CHECK");

        Assert.True(turn.IsAuthoritativeTurn);
        Assert.Equal("checker", turn.AuthoritativeOwner);
        Assert.Equal("I AM the Checker Agent.", turn.IdentityStatement);
    }

    [Fact]
    public void Dialogue_turn_carries_explicit_dependency_and_next_handoff()
    {
        var turn = new CognitiveDialogueTurn(
            TurnId: "T3",
            SpeakerRole: "checker",
            CognitiveLens: "CHECK",
            AddressedTo: ["reflector"],
            Statement: "The implementation is independently verified.",
            Question: null,
            Challenge: null,
            Response: "PASS",
            EvidenceRefs: ["evidence://check-3"],
            PriorBeliefRefs: [],
            RevisedBelief: "VerifiedState is now PASS.",
            UnresolvedTensions: [],
            SelfReference: null,
            AuthoritativeFrameRef: "checker:CHECK",
            DependencyRelation: DialogueDependencyRelations.Verifies,
            DependsOnTurnRefs: ["T2"],
            NextSpeakerRole: "reflector",
            NextDependency: "Crystallize True Intent and emit the next Seed reference.");

        Assert.Equal("I AM the Checker Agent.", turn.IdentityStatement);
        Assert.Equal(DialogueDependencyRelations.Verifies, turn.DependencyRelation);
        Assert.Contains("T2", turn.DependsOn);
        Assert.Equal("reflector", turn.NextSpeakerRole);
        Assert.True(turn.HasCausalContinuation);
        Assert.False(turn.GrantsAuthority);
    }

    [Fact]
    public void Reflector_can_close_cycle_with_exact_next_seed_reference()
    {
        var turn = new CognitiveDialogueTurn(
            TurnId: "T4", SpeakerRole: "reflector", CognitiveLens: "REFLECT",
            AddressedTo: ["orchestrator"], Statement: "True Intent crystallized.",
            Question: null, Challenge: null, Response: null,
            EvidenceRefs: ["evidence://checker-pass"], PriorBeliefRefs: [], RevisedBelief: null,
            UnresolvedTensions: [], SelfReference: null, AuthoritativeFrameRef: "reflector:REFLECT",
            DependencyRelation: DialogueDependencyRelations.Accepts,
            DependsOnTurnRefs: ["T3"], NextSpeakerRole: "orchestrator",
            NextDependency: "Re-admit the emitted Seed in a fresh Cycle.", NextSeedRef: "seed://next-1");

        Assert.Equal("I AM the Reflector Agent.", turn.IdentityStatement);
        Assert.Equal("seed://next-1", turn.NextSeedRef);
        Assert.True(turn.HasCausalContinuation);
    }
}
