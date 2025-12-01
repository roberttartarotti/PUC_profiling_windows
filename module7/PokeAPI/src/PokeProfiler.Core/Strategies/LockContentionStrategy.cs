using PokeProfiler.Core.Instrumentation;

namespace PokeProfiler.Core.Strategies;

// Intentionally creates lock contention by using coarse-grained lock
public class LockContentionStrategy : IPokemonFetchStrategy
{
    public string Name => "Lock Contention";
    private readonly PokeApiClient _client;
    private readonly object _lock = new();
    public LockContentionStrategy(PokeApiClient client) => _client = client;

    public async Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> idsOrNames, CancellationToken ct = default)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("LockContentionStrategy.FetchAsync");
        var list = new List<Pokemon>();
        var tasks = idsOrNames.Select(async id =>
        {
            var p = await _client.GetPokemonAsync(id, ct);
            if (p != null)
            {
                // Hot lock to serialize writes
                using var lockActivity = Telemetry.ActivitySource.StartActivity($"Lock-{id}");
                lock (_lock)
                {
                    // Simulate extra CPU inside lock
                    BusyWork(5000);
                    list.Add(p);
                }
            }
        });
        await Task.WhenAll(tasks);
        return list;
    }

    private static void BusyWork(int iterations)
    {
        int x = 0;
        for (int i = 0; i < iterations; i++)
            x ^= (i * 31) % 7;
    }
}
