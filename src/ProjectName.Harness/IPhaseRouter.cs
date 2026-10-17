namespace ProjectName.Harness;

using ProjectName.Contracts;

/// <summary>Routes admitted cycles through ORCHESTRATE → PLAN → MAKE → CHECK → REFLECT.
/// Does not invent grants or activate frameworks.</summary>
public interface IPhaseRouter
{
    Task<object> DispatchAsync(PmcroRequest request, CancellationToken ct = default);
}
