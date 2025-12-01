using PokeProfiler.Core.Instrumentation;

namespace PokeProfiler.Core.Strategies;

public class SequentialStrategy : IPokemonFetchStrategy
{
    public string Name => "Sequential";
    private readonly PokeApiClient _client;
    public SequentialStrategy(PokeApiClient client) => _client = client;

    public async Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> idsOrNames, CancellationToken ct = default)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("SequentialStrategy.FetchAsync");
        var results = new List<Pokemon>();
        foreach (var id in idsOrNames)
        {
            using var itemActivity = Telemetry.ActivitySource.StartActivity($"Fetch-{id}");
            var p = await _client.GetPokemonAsync(id, ct);
            if (p != null) results.Add(p);
        }
        return results;
    }
}
