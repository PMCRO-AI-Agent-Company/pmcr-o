using ProjectName.Contracts;
using ProjectName.Core;
using Xunit;

namespace ProjectName.Runtime.Tests;

public class IngressProtocolTests
{
    [Fact]
    public void Raw_text_is_rejected_with_corrected_seed_route()
    {
        IPmcroIngressProtocol protocol = new PmcroIngressProtocol();
        var result = Assert.IsType<PmcroIngressRejection>(protocol.ParseOrReject("build me an agent"));
        Assert.Equal("REJECTED", result.Status);
        Assert.False(result.Admitted);
        Assert.Equal("@orchestrator /pmcro:seed \"build me an agent\"", result.CorrectedRoute);
    }

    [Fact]
    public void Structured_seed_route_is_accepted_as_ingress_not_cycle()
    {
        IPmcroIngressProtocol protocol = new PmcroIngressProtocol();
        var result = Assert.IsType<ParsedPmcroRoute>(
            protocol.ParseOrReject("/pmcro:seed \"build me an agent\""));
        Assert.Equal("orchestrator", result.Agent);
        Assert.Equal("seed", result.Command);
        Assert.Equal("build me an agent", result.Intent);
    }

    [Fact]
    public void Raw_rejection_is_guidance_not_admission()
    {
        IPmcroIngressProtocol protocol = new PmcroIngressProtocol();
        var result = Assert.IsType<PmcroIngressRejection>(protocol.ParseOrReject("make app maybe docker and mobile idk"));
        Assert.False(result.Admitted);
        Assert.Contains("guidance", result.Guidance, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("@orchestrator /pmcro:seed \"make app maybe docker and mobile idk\"", result.CorrectedRoute);
    }

    [Fact]
    public void Messy_seed_payload_is_accepted_without_requiring_preplanning()
    {
        IPmcroIngressProtocol protocol = new PmcroIngressProtocol();
        var result = Assert.IsType<ParsedPmcroRoute>(protocol.ParseOrReject(
            "/pmcro:seed \"make app maybe docker and mobile idk\""));
        Assert.Equal("seed", result.Command);
        Assert.Equal("make app maybe docker and mobile idk", result.Intent);
    }

    [Fact]
    public void Init_opens_cycle_one_and_points_to_orchestrate()
    {
        IPmcroIngressProtocol protocol = new PmcroIngressProtocol();
        var ack = protocol.Initialize(new PmcroInitRequest(
            "build an agent system", TrailRef: "T001", ProjectId: "Demo"));

        Assert.Equal(1, ack.CycleNumber);
        Assert.Equal("T001-C001", ack.CycleId);
        Assert.Equal("T001-S001", ack.SeedId);
        Assert.Equal("Cycle 1 initialized", ack.Status);
        Assert.Equal("@orchestrator /pmcro:orchestrate", ack.NextRoute);
    }

    [Fact]
    public void Reinitializing_same_trail_is_rejected_in_favor_of_seed()
    {
        IPmcroIngressProtocol protocol = new PmcroIngressProtocol();
        protocol.Initialize(new PmcroInitRequest("first", TrailRef: "T001"));
        var ex = Assert.Throws<InvalidOperationException>(() =>
            protocol.Initialize(new PmcroInitRequest("second", TrailRef: "T001")));
        Assert.Contains("/pmcro:seed", ex.Message);
    }

    [Fact]
    public void External_request_without_trail_gets_init_as_first_response()
    {
        IPmcroIngressProtocol protocol = new PmcroIngressProtocol();
        var response = protocol.CreateFirstResponse("build me a mobile app", trailActive: false);
        Assert.Equal("init", response.Command);
        Assert.Equal("@orchestrator /pmcro:init \"build me a mobile app\"", response.Route);
        Assert.False(response.AdmittedByResponse);
    }

    [Fact]
    public void External_request_with_active_trail_gets_seed_as_first_response()
    {
        IPmcroIngressProtocol protocol = new PmcroIngressProtocol();
        var response = protocol.CreateFirstResponse("add docker maybe github too", trailActive: true);
        Assert.Equal("seed", response.Command);
        Assert.Equal("@orchestrator /pmcro:seed \"add docker maybe github too\"", response.Route);
        Assert.False(response.AdmittedByResponse);
    }

}

