namespace ProjectName.Harness;

using Microsoft.Extensions.DependencyInjection;
using ProjectName.Application.Abstractions;
using ProjectName.Capabilities;
using ProjectName.Core;
using ProjectName.Contracts;
using ProjectName.Infrastructure;
using ProjectName.Marketplace;
using ProjectName.Runtime.Abstractions;
using ProjectName.Runtime.Maf;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectNameAgentSystem(
        this IServiceCollection services,
        AutoRunOptions? autoRun = null,
        string? mcpCatalogPath = null,
        int frameStartOrdinal = 0,
        IAgentRuntime? agentRuntime = null,
        string? messageBusRoot = null,
        string messageBusWorkerId = "pmcro-session")
    {
        services.AddSingleton(autoRun ?? new AutoRunOptions());
        services.AddSingleton<IPmcroIngressProtocol, PmcroIngressProtocol>();
        services.AddSingleton<IPmcroOperations, PmcroOperations>();
        services.AddSingleton<IFrameworkActivation, FrameworkActivationStub>();
        services.AddSingleton<ICapabilityResolver>(_ => new McpCapabilityResolver(mcpCatalogPath));
        services.AddSingleton<ICapabilityInvoker, ScriptCapabilityInvoker>();
        services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();
        services.AddSingleton<IResourceValidator, MarketplaceResourceValidator>();
        services.AddSingleton<IPhaseHandler, DefaultPhaseHandler>();
        services.AddSingleton<IPhaseRouter, PhaseRouter>();
        services.AddSingleton<IFrameSequencer>(_ => new FrameSequencer(frameStartOrdinal));
        services.AddSingleton<ICycleErrorPath, CycleErrorPath>();
        services.AddSingleton<ISeedController, SeedController>();
        if (string.IsNullOrWhiteSpace(messageBusRoot))
        {
            services.AddSingleton<ISeedQueue, InMemorySeedQueue>();
        }
        else
        {
            services.AddSingleton<IMessageBus>(_ => new FilePmcroMessageBus(messageBusRoot));
            services.AddSingleton<ISeedQueue>(sp => new MessageBusSeedQueue(
                sp.GetRequiredService<IMessageBus>(), messageBusWorkerId));
            services.AddSingleton<TrailAutomationService>();
        }
        services.AddSingleton<IHostFrameworkAdapter, MafHostAdapter>();
        if (agentRuntime is not null)
            services.AddSingleton<IAgentRuntime>(agentRuntime);
        return services;
    }

    public static IServiceCollection AddProjectNameProductionActivation(
        this IServiceCollection services,
        string activationRoot,
        ISecretProvider secrets,
        ICapabilityRegistrationStore registrations,
        IGrantStore grants,
        IRevocationStore revocations,
        string keyProvider,
        string keyLocationRef)
    {
        services.AddSingleton(secrets);
        services.AddSingleton(registrations);
        services.AddSingleton(grants);
        services.AddSingleton(revocations);
        services.AddSingleton<IActivationStore>(_ => new FileActivationStore(activationRoot));
        services.AddSingleton(sp => new ProductionGrantVerifier(secrets, grants, revocations));
        services.AddSingleton<IFrameworkActivation>(sp => new AgentActivationGate(
            sp.GetRequiredService<ProductionGrantVerifier>(), registrations, revocations,
            sp.GetRequiredService<IActivationStore>(), keyProvider, keyLocationRef));
        return services;
    }

    public static IServiceCollection AddProjectNamePmcroRuntime(
        this IServiceCollection services,
        AutoRunOptions? autoRun = null,
        string? mcpCatalogPath = null,
        int frameStartOrdinal = 0,
        IAgentRuntime? agentRuntime = null,
        string? messageBusRoot = null,
        string messageBusWorkerId = "pmcro-session") =>
        services.AddProjectNameAgentSystem(autoRun, mcpCatalogPath, frameStartOrdinal, agentRuntime, messageBusRoot, messageBusWorkerId);
}
