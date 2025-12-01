using PokeProfiler.Core.Instrumentation;

namespace PokeProfiler.Core.Strategies;

public class TaskWhenAllStrategy : IPokemonFetchStrategy
{
    public string Name => "Task.WhenAll";
    private readonly PokeApiClient _client;
    public TaskWhenAllStrategy(PokeApiClient client) => _client = client;

    public async Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> idsOrNames, CancellationToken ct = default)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("TaskWhenAllStrategy.FetchAsync");
        var tasks = idsOrNames.Select(id => _client.GetPokemonAsync(id, ct));
        var res = await Task.WhenAll(tasks);
        return res.Where(p => p != null).ToList()!;
    }
}
