using PokeProfiler.Core.Instrumentation;
using System.Text.RegularExpressions;

namespace PokeProfiler.Core.Strategies;

// Intentionally uses inefficient regex and string operations
public class IneffAlgorithmStrategy : IPokemonFetchStrategy
{
    public string Name => "Inefficient Algorithm";
    private readonly PokeApiClient _client;

    public IneffAlgorithmStrategy(PokeApiClient client) => _client = client;

    public async Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> idsOrNames, CancellationToken ct = default)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("IneffAlgorithmStrategy.FetchAsync");
        var results = new List<Pokemon>();

        foreach (var id in idsOrNames)
        {
            var p = await _client.GetPokemonAsync(id, ct);
            if (p != null)
            {
                // Inefficient: recreate regex on every iteration
                var regex = new Regex(@"[a-zA-Z]+");
                var matches = regex.Matches(p.name);

                // Inefficient: O(n²) string concatenation in loop
                string processed = "";
                for (int i = 0; i < 1000; i++)
                {
                    processed += $"{p.name}-{i},"; // String concat instead of StringBuilder
                }

                // Inefficient: unnecessary LINQ operations
                var temp = Enumerable.Range(0, 100)
                    .Select(x => x.ToString())
                    .Where(x => int.Parse(x) > 50)
                    .OrderBy(x => x)
                    .ToList();

                results.Add(p);
            }
        }

        return results;
    }
}
