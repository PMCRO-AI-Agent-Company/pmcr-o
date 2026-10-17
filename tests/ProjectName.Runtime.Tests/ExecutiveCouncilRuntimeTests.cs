using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using ProjectName.Contracts;
using ProjectName.Runtime.Maf;
using Xunit;

namespace ProjectName.Runtime.Tests;

public sealed class ExecutiveCouncilRuntimeTests
{
    [Fact]
    public void Human_request_becomes_non_authoritative_trail_message()
    {
        var port = new RequestPort("owner-approval", typeof(string), typeof(bool));
        var external = ExternalRequest.Create(port, "Approve the proposed executive action?", "req-1");
        var evt = new RequestInfoEvent(external);

        var message = MafExecutiveHumanDecisionBridge.ToMessage(
            evt, "trail-1", "executive-council", "owner", "seed-1", "cause-1", "corr-1");

        Assert.Equal(PmcroMessageKinds.HumanDecisionRequest, message.Kind);
        Assert.Equal("trail-1", message.TrailId);
        Assert.Equal("seed-1", message.SeedRef);
        Assert.False(message.GrantsAuthority);
        using var payload = JsonDocument.Parse(message.PayloadJson);
        Assert.Equal("req-1", payload.RootElement.GetProperty("requestId").GetString());
        Assert.Equal("owner-approval", payload.RootElement.GetProperty("portId").GetString());
    }

    [Fact]
    public async Task File_checkpoint_store_can_be_reopened_by_fresh_instance()
    {
        using var temp = new TempDir();
        string root;
        using (var first = new MafExecutiveCheckpointStore(temp.Path))
        {
            root = first.Root;
            Assert.True(Directory.Exists(root));
            Assert.Null(await first.GetLatestAsync("session-1"));
        }

        using var second = new MafExecutiveCheckpointStore(root);
        Assert.Equal(root, second.Root);
        Assert.Null(await second.GetLatestAsync("session-1"));
    }

    [Fact]
    public void Executive_compiler_exposes_real_handoff_projection_path()
    {
        var method = typeof(MafExecutiveCouncilCompiler).GetMethod(nameof(MafExecutiveCouncilCompiler.CompileHandoff));
        Assert.NotNull(method);
        Assert.Equal(typeof(MafExecutiveCouncilProjection), method!.ReturnType);
    }

    [Fact]
    public void Fresh_workflow_runtime_uses_real_file_checkpointing_and_resume_path()
    {
        using var temp = new TempDir();
        string root;
        using (var first = new MafExecutiveWorkflowRuntime(temp.Path))
        {
            Assert.True(first.IsCheckpointingEnabled);
            root = first.CheckpointRoot;
        }

        using var second = new MafExecutiveWorkflowRuntime(root);
        Assert.True(second.IsCheckpointingEnabled);
        Assert.Equal(root, second.CheckpointRoot);
        var resume = typeof(MafExecutiveWorkflowRuntime).GetMethod(nameof(MafExecutiveWorkflowRuntime.ResumeAsync));
        Assert.NotNull(resume);
    }

    private sealed class TempDir : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "pmcro-csuite-runtime-" + Guid.NewGuid().ToString("N"));
        public TempDir() => Directory.CreateDirectory(Path);
        public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, true); }
    }
}
