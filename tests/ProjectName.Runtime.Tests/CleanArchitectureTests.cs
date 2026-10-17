using ProjectName.Application.Abstractions;
using ProjectName.Capabilities;
using ProjectName.Contracts;
using ProjectName.Domain;
using ProjectName.Infrastructure;
using Xunit;

namespace ProjectName.Runtime.Tests;

public class CleanArchitectureTests
{
    private sealed class SampleEntity : GuidEntity
    {
        public SampleEntity(string name) => Name = name;
        public string Name { get; private set; }
        public void Rename(string name) => Name = name;
    }

    [Fact]
    public async Task Generic_repository_and_unit_of_work_are_reusable()
    {
        IUnitOfWork uow = new InMemoryUnitOfWork();
        var repo = uow.Repository<SampleEntity, Guid>();
        var entity = new SampleEntity("first");
        await repo.AddAsync(entity);
        Assert.Equal(1, await uow.SaveChangesAsync());
        Assert.Equal(entity.Id, (await repo.GetByIdAsync(entity.Id))!.Id);

        entity.Rename("second");
        repo.Update(entity);
        Assert.Equal(1, await uow.SaveChangesAsync());
        Assert.Equal("second", (await repo.GetByIdAsync(entity.Id))!.Name);
    }

    [Fact]
    public async Task Model_backed_invocation_requires_effective_grant()
    {
        var script = Path.GetTempFileName() + ".ps1";
        await File.WriteAllTextAsync(script, "exit 0");
        try
        {
            ICapabilityInvoker invoker = new ScriptCapabilityInvoker(new McpCapabilityResolver());
            var request = new CapabilityInvocationRequest("example.install", script, "simulate", Caller: "model:test");
            var denied = new AgentExecutionGrant([], [], [], [], "test");
            var deniedResult = await invoker.InvokeAsync(request, denied);
            Assert.False(deniedResult.Ok);
            Assert.Equal("capability-not-granted", deniedResult.Status);

            var allowedIds = new[] { "capability.invoke", "example.install" };
            var allowed = new AgentExecutionGrant(allowedIds, allowedIds, allowedIds, [], "test");
            var result = await invoker.InvokeAsync(request, allowed);
            Assert.True(result.Ok);
            Assert.Equal("simulation-pass", result.Status);
        }
        finally
        {
            if (File.Exists(script)) File.Delete(script);
        }
    }
}
