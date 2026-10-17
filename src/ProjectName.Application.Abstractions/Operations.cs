namespace ProjectName.Application.Abstractions;

using ProjectName.Contracts;

public interface IOperationHandler<in TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken ct = default);
}

public interface ICapabilityInvoker
{
    Task<CapabilityInvocationResult> InvokeAsync(
        CapabilityInvocationRequest request,
        AgentExecutionGrant grant,
        CancellationToken ct = default);
}
