using Microsoft.Data.Sqlite;

namespace PokeProfiler.Core.Persistence;

public class PokemonRepository
{
    private readonly string _dbPath;
    public PokemonRepository(string dbPath)
    {
        _dbPath = dbPath;
        Initialize();
    }

    private void Initialize()
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS pokemon (
            id INTEGER PRIMARY KEY,
            name TEXT NOT NULL,
            height INTEGER,
            weight INTEGER,
            fetched_at TEXT NOT NULL
        );";
        cmd.ExecuteNonQuery();
    }

    public void Upsert(Pokemon p)
    {
        using var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO pokemon(id, name, height, weight, fetched_at)
            VALUES($id, $name, $height, $weight, $fetched)
            ON CONFLICT(id) DO UPDATE SET name=$name, height=$height, weight=$weight, fetched_at=$fetched;";
        cmd.Parameters.AddWithValue("$id", p.id);
        cmd.Parameters.AddWithValue("$name", p.name);
        cmd.Parameters.AddWithValue("$height", p.height);
        cmd.Parameters.AddWithValue("$weight", p.weight);
        cmd.Parameters.AddWithValue("$fetched", DateTime.UtcNow.ToString("O"));
        cmd.ExecuteNonQuery();
    }
}
