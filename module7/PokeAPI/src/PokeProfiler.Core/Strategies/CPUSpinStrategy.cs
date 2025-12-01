using PokeProfiler.Core.Instrumentation;

namespace PokeProfiler.Core.Strategies;

// Intentionally creates CPU spinning with busy wait
public class CPUSpinStrategy : IPokemonFetchStrategy
{
    public string Name => "CPU Spin";
    private readonly PokeApiClient _client;

    public CPUSpinStrategy(PokeApiClient client) => _client = client;

    public async Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> idsOrNames, CancellationToken ct = default)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("CPUSpinStrategy.FetchAsync");
        var results = new List<Pokemon>();

        foreach (var id in idsOrNames)
        {
            // Busy-wait instead of proper async: burns CPU
            var task = _client.GetPokemonAsync(id, ct);
            while (!task.IsCompleted)
            {
                // Spin-wait burns CPU instead of yielding
                Thread.SpinWait(1000);
            }

            var p = await task;
            if (p != null) results.Add(p);
        }

        return results;
    }
}
