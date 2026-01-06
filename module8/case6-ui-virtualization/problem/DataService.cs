using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UIVirtualization.Problem;

public class DataService
{
    public List<Cliente> CarregarClientes()
    {
        // Usa o CSV do case4
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var csvPath = Path.Combine(baseDir, "..", "..", "..", "..", "case4-wpf-startup-performance", "data", "clientes-prod.csv");
        
        // Fallback para dev
        if (!File.Exists(csvPath))
        {
            csvPath = Path.Combine(baseDir, "..", "..", "..", "..", "case4-wpf-startup-performance", "data", "clientes-dev.csv");
        }

        if (!File.Exists(csvPath))
        {
            // Gera dados mock se CSV não existir
            return GerarClientesMock(50000);
        }

        var clientes = new List<Cliente>();
        var lines = File.ReadAllLines(csvPath);

        // Limita a 10.000 clientes para demonstração
        int maxClientes = Math.Min(10000, lines.Length - 1);

        // Pula header
        for (int i = 1; i <= maxClientes; i++)
        {
            var parts = lines[i].Split(',');
            if (parts.Length >= 6)
            {
                clientes.Add(new Cliente
                {
                    Id = int.Parse(parts[0]),
                    Nome = parts[1],
                    Email = parts[2],
                    Telefone = parts[3],
                    Cidade = parts[4],
                    Estado = parts[5]
                });
            }
        }

        return clientes;
    }

    private List<Cliente> GerarClientesMock(int count)
    {
        // Limita a 10.000 para demonstração
        count = Math.Min(10000, count);
        var clientes = new List<Cliente>();
        for (int i = 1; i <= count; i++)
        {
            clientes.Add(new Cliente
            {
                Id = i,
                Nome = $"Cliente {i}",
                Email = $"cliente{i}@email.com",
                Telefone = $"(11) 9{i:D4}-{i % 10000:D4}",
                Cidade = "São Paulo",
                Estado = "SP"
            });
        }
        return clientes;
    }
}
