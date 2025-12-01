using PokeProfiler.Core.Instrumentation;

namespace PokeProfiler.Core.Strategies;

// Intentionally creates memory leak by accumulating references
public class MemoryLeakStrategy : IPokemonFetchStrategy
{
    public string Name => "Memory Leak";
    private readonly PokeApiClient _client;
    private static readonly List<byte[]> _leakedMemory = new(); // Never cleared

    public MemoryLeakStrategy(PokeApiClient client) => _client = client;

    public async Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> idsOrNames, CancellationToken ct = default)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("MemoryLeakStrategy.FetchAsync");
        var results = new List<Pokemon>();

        foreach (var id in idsOrNames)
        {
            var p = await _client.GetPokemonAsync(id, ct);
            if (p != null)
            {
                results.Add(p);
                // Intentional memory leak: allocate 1MB per pokemon and never release
                _leakedMemory.Add(new byte[1024 * 1024]);
            }
        }

        return results;
    }
}
