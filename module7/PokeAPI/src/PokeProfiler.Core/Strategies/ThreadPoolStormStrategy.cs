using PokeProfiler.Core.Instrumentation;
using System.Collections.Concurrent;

namespace PokeProfiler.Core.Strategies;

// Intentionally bad: floods ThreadPool with Task.Run causing contention
public class ThreadPoolStormStrategy : IPokemonFetchStrategy
{
    public string Name => "ThreadPool Storm";
    private readonly PokeApiClient _client;
    public ThreadPoolStormStrategy(PokeApiClient client) => _client = client;

    public async Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> idsOrNames, CancellationToken ct = default)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("ThreadPoolStormStrategy.FetchAsync");
        var bag = new ConcurrentBag<Pokemon>();
        var tasks = new List<Task>();
        // Oversubscribe naive parallelism
        foreach (var id in idsOrNames)
        {
            tasks.Add(Task.Run(async () =>
            {
                using var itemActivity = Telemetry.ActivitySource.StartActivity($"TaskRun-{id}");
                var p = await _client.GetPokemonAsync(id, ct);
                if (p != null) bag.Add(p);
            }));
        }
        await Task.WhenAll(tasks);
        return bag.ToList();
    }
}
