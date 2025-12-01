using PokeProfiler.Core.Instrumentation;
using System.Collections.Concurrent;

namespace PokeProfiler.Core.Strategies;

// Intentionally creates deadlock scenario with multiple locks
public class DeadlockRiskStrategy : IPokemonFetchStrategy
{
    public string Name => "Deadlock Risk";
    private readonly PokeApiClient _client;
    private readonly object _lock1 = new();
    private readonly object _lock2 = new();

    public DeadlockRiskStrategy(PokeApiClient client) => _client = client;

    public async Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> idsOrNames, CancellationToken ct = default)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("DeadlockRiskStrategy.FetchAsync");
        var list = new ConcurrentBag<Pokemon>();
        var tasks = new List<Task>();
        var ids = idsOrNames.ToArray();

        // Create potential deadlock with reverse lock ordering
        for (int i = 0; i < ids.Length; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                var id = ids[index];
                var p = await _client.GetPokemonAsync(id, ct);

                if (p != null)
                {
                    // Even indices: lock1 then lock2
                    // Odd indices: lock2 then lock1 (deadlock potential)
                    if (index % 2 == 0)
                    {
                        lock (_lock1)
                        {
                            Thread.Sleep(10); // Increase deadlock window
                            lock (_lock2)
                            {
                                list.Add(p);
                            }
                        }
                    }
                    else
                    {
                        lock (_lock2)
                        {
                            Thread.Sleep(10); // Increase deadlock window
                            lock (_lock1)
                            {
                                list.Add(p);
                            }
                        }
                    }
                }
            }, ct));
        }

        await Task.WhenAll(tasks);
        return list.ToList();
    }
}
