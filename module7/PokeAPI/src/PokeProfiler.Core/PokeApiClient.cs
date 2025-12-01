using System.Text.Json;

namespace PokeProfiler.Core;

public class PokeApiClient
{
    private readonly HttpClient _httpClient;
    public int ArtificialDelayMs { get; set; }

    public PokeApiClient(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient
        {
            BaseAddress = new Uri("https://pokeapi.co/api/v2/")
        };
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("PUC.PokeProfiler/1.0");
    }

    public async Task<Pokemon?> GetPokemonAsync(string nameOrId, CancellationToken ct = default)
    {
        if (ArtificialDelayMs > 0)
            await Task.Delay(ArtificialDelayMs, ct);

        using var resp = await _httpClient.GetAsync($"pokemon/{nameOrId}", ct);
        resp.EnsureSuccessStatusCode();
        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        var pokemon = await JsonSerializer.DeserializeAsync<Pokemon>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }, ct);
        return pokemon;
    }
}

public record Pokemon(int id, string name, int height, int weight);
