using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace StartupPerformance.Problem;

public class DataService
{
    private readonly string _csvPath;

    public DataService(string csvFileName = "clientes-dev.csv")
    {
        // Procura o CSV na pasta data (pode estar em diferentes níveis)
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

    // PROBLEMA: Leitura síncrona que bloqueia a thread
    public List<Cliente> CarregarClientesSincrono()
    {
        var clientes = new List<Cliente>();
        var lines = File.ReadAllLines(_csvPath);

        // Pula header
        for (int i = 1; i < lines.Length; i++)
        {
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

            // Simula processamento adicional (validações, transformações)
            if (i % 100 == 0)
            {
                Thread.Sleep(20); // Adiciona latência para simular processamento
            }
        }

        return clientes;
    }
}
