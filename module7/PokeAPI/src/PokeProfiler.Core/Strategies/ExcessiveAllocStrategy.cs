using PokeProfiler.Core.Instrumentation;

namespace PokeProfiler.Core.Strategies;

// Intentionally creates excessive allocations and boxing
public class ExcessiveAllocStrategy : IPokemonFetchStrategy
{
    public string Name => "Excessive Alloc";
    private readonly PokeApiClient _client;

    public ExcessiveAllocStrategy(PokeApiClient client) => _client = client;

    public async Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> idsOrNames, CancellationToken ct = default)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("ExcessiveAllocStrategy.FetchAsync");
        var results = new List<Pokemon>();

        foreach (var id in idsOrNames)
        {
            // Excessive allocation: create many temporary strings and lists
            for (int i = 0; i < 1000; i++)
            {
                var temp = new List<string>();
                temp.Add($"Waste-{i}-{id}-{Guid.NewGuid()}");
                temp.Add(DateTime.Now.ToString());
                var combined = string.Join(",", temp); // String allocation
            }

            var p = await _client.GetPokemonAsync(id, ct);
            if (p != null) results.Add(p);
        }

        return results;
    }
}
