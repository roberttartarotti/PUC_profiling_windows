using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StartupPerformance.Solved1;

public class DataService
{
    private readonly string _csvPath;

    public DataService(string csvFileName = "clientes-dev.csv")
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var possiblePaths = new[]
        {
            Path.Combine(baseDir, "..", "..", "..", "..", "data", csvFileName),
            Path.Combine(baseDir, "..", "..", "..", "data", csvFileName),
            Path.Combine(baseDir, "data", csvFileName),
            Path.Combine(baseDir, csvFileName)
        };

        _csvPath = possiblePaths.FirstOrDefault(File.Exists) 
                   ?? throw new FileNotFoundException($"CSV não encontrado: {csvFileName}");
    }

    // SOLUÇÃO: Leitura assíncrona com progresso
    public async Task<List<Cliente>> CarregarClientesAsync(
        IProgress<(int current, int total, string message)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(async () =>
        {
            var clientes = new List<Cliente>();
            var lines = await File.ReadAllLinesAsync(_csvPath, cancellationToken);
            var totalLines = lines.Length - 1; // Exclui header

            progress?.Report((0, totalLines, "Iniciando carregamento..."));

            // Pula header
            for (int i = 1; i < lines.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var parts = lines[i].Split(',');
                if (parts.Length >= 8)
                {
                    clientes.Add(new Cliente
                    {
                        Id = int.Parse(parts[0]),
                        Nome = parts[1],
                        Email = parts[2],
                        Telefone = parts[3],
                        Cidade = parts[4],
                        Estado = parts[5],
                        DataCadastro = parts[6],
                        Status = parts[7]
                    });
                }

                // Reporta progresso a cada 100 registros
                if (i % 100 == 0)
                {
                    var percentage = (i * 100) / totalLines;
                    progress?.Report((i, totalLines, $"Carregando... {percentage}%"));
                    await Task.Delay(20, cancellationToken); // Simula processamento + cooperação
                }
            }

            progress?.Report((totalLines, totalLines, "Carregamento concluído!"));
            return clientes;
        }, cancellationToken);
    }
}
