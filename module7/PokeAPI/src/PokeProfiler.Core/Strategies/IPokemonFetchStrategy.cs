namespace PokeProfiler.Core.Strategies;

public interface IPokemonFetchStrategy
{
    string Name { get; }
    Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> idsOrNames, CancellationToken ct = default);
}
