using PokeProfiler.Core.Instrumentation;
using System.Collections.Concurrent;

namespace PokeProfiler.Core.Strategies;

// Controlled concurrency with SemaphoreSlim to compare with unbounded Task.WhenAll
public class SemaphoreBatchStrategy : IPokemonFetchStrategy
{
    public string Name => "Semaphore Batch (10)";
    private readonly PokeApiClient _client;
    private readonly int _maxConcurrency;

    public SemaphoreBatchStrategy(PokeApiClient client, int maxConcurrency = 10)
    {
        _client = client;
        _maxConcurrency = maxConcurrency;
    }

    public async Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> idsOrNames, CancellationToken ct = default)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("SemaphoreBatchStrategy.FetchAsync");
        var bag = new ConcurrentBag<Pokemon>();
        var sem = new SemaphoreSlim(_maxConcurrency);
        var tasks = idsOrNames.Select(async id =>
        {
            await sem.WaitAsync(ct);
            try
            {
                using var itemActivity = Telemetry.ActivitySource.StartActivity($"Semaphore-{id}");
                var p = await _client.GetPokemonAsync(id, ct);
                if (p != null) bag.Add(p);
            }
            finally
            {
                sem.Release();
            }
        });
        await Task.WhenAll(tasks);
        return bag.ToList();
    }
}
