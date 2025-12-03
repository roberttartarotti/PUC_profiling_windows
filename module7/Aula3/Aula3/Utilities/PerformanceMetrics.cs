using System.Diagnostics;
using System.Text.Json;

namespace Aula3.Utilities;

/// <summary>
/// Sistema de coleta de métricas de performance para integração em CI/CD
/// Slide 9: Perfilamento integrado no pipeline de desenvolvimento
/// </summary>
public class PerformanceMetrics
{
    public string TestName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public long MemoryUsedBytes { get; set; }
    public double ThroughputPerSecond { get; set; }
    public int ThreadCount { get; set; }
    public Dictionary<string, object> CustomMetrics { get; set; } = [];

    public override string ToString()
    {
        return $"{TestName}: {ElapsedMilliseconds}ms, Memory: {MemoryUsedBytes / 1024}KB, " +
               $"Throughput: {ThroughputPerSecond:F2}/s, Threads: {ThreadCount}";
    }
}

/// <summary>
/// Coletor de métricas para testes de performance
/// Uso: using var collector = new PerformanceMetricsCollector("TestName");
/// </summary>
public class PerformanceMetricsCollector : IDisposable
{
    private readonly string _testName;
    private readonly Stopwatch _stopwatch;
    private readonly long _initialMemory;
    private readonly int _initialThreadCount;
    private int _operationCount;

    public PerformanceMetricsCollector(string testName)
    {
        _testName = testName;
        
        // Força GC para ter medição mais precisa
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        _initialMemory = GC.GetTotalMemory(false);
        _initialThreadCount = Process.GetCurrentProcess().Threads.Count;
        _stopwatch = Stopwatch.StartNew();
    }

    public void RecordOperation()
    {
        Interlocked.Increment(ref _operationCount);
    }

    public void RecordOperations(int count)
    {
        Interlocked.Add(ref _operationCount, count);
    }

    public PerformanceMetrics GetMetrics()
    {
        _stopwatch.Stop();
        
        var finalMemory = GC.GetTotalMemory(false);
        var memoryUsed = Math.Max(0, finalMemory - _initialMemory);
        var throughput = _operationCount / _stopwatch.Elapsed.TotalSeconds;

        return new PerformanceMetrics
        {
            TestName = _testName,
            Timestamp = DateTime.UtcNow,
            ElapsedMilliseconds = _stopwatch.ElapsedMilliseconds,
            MemoryUsedBytes = memoryUsed,
            ThroughputPerSecond = throughput,
            ThreadCount = Process.GetCurrentProcess().Threads.Count - _initialThreadCount
        };
    }

    public void Dispose()
    {
        var metrics = GetMetrics();
        PerformanceMetricsReporter.Instance.Report(metrics);
    }
}

/// <summary>
/// Reporter central de métricas com suporte a baseline e alertas
/// </summary>
public class PerformanceMetricsReporter
{
    private static readonly Lazy<PerformanceMetricsReporter> _instance = new(() => new PerformanceMetricsReporter());
    public static PerformanceMetricsReporter Instance => _instance.Value;

    private readonly List<PerformanceMetrics> _metrics = [];
    private readonly Dictionary<string, PerformanceMetrics> _baselines = [];

    public void Report(PerformanceMetrics metrics)
    {
        _metrics.Add(metrics);
        Console.WriteLine($"\n[METRICA] {metrics}");

        // Verifica se há baseline para comparação
        if (_baselines.TryGetValue(metrics.TestName, out var baseline))
        {
            CompareWithBaseline(metrics, baseline);
        }
    }

    public void SetBaseline(string testName, PerformanceMetrics baseline)
    {
        _baselines[testName] = baseline;
        Console.WriteLine($"? Baseline definida para '{testName}'");
    }

    public void LoadBaselinesFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"[ARQUIVO] Arquivo de baselines não encontrado: {filePath}");
            return;
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var baselines = JsonSerializer.Deserialize<List<PerformanceMetrics>>(json);
            
            if (baselines != null)
            {
                foreach (var baseline in baselines)
                {
                    _baselines[baseline.TestName] = baseline;
                }
                Console.WriteLine($"? {baselines.Count} baselines carregadas de {filePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"?? Erro ao carregar baselines: {ex.Message}");
        }
    }

    public void SaveBaselinesToFile(string filePath)
    {
        try
        {
            var json = JsonSerializer.Serialize(_baselines.Values.ToList(), new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(filePath, json);
            Console.WriteLine($"[ARQUIVO] Baselines salvas em {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO] Erro ao salvar baselines: {ex.Message}");
        }
    }

    public void ExportMetricsToJson(string filePath)
    {
        var json = JsonSerializer.Serialize(_metrics, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(filePath, json);
        Console.WriteLine($"\n[ARQUIVO] Métricas exportadas para {filePath}");
    }

    public void GenerateReport()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("RELATÓRIO DE PERFORMANCE");
        Console.WriteLine(new string('=', 80));

        foreach (var metric in _metrics)
        {
            Console.WriteLine($"\n{metric.TestName}:");
            Console.WriteLine($"  Tempo: {metric.ElapsedMilliseconds} ms");
            Console.WriteLine($"  Memória: {metric.MemoryUsedBytes / 1024.0:F2} KB");
            Console.WriteLine($"  Throughput: {metric.ThroughputPerSecond:F2} ops/s");
            Console.WriteLine($"  Threads: {metric.ThreadCount}");

            if (metric.CustomMetrics.Any())
            {
                Console.WriteLine("  Métricas Customizadas:");
                foreach (var custom in metric.CustomMetrics)
                {
                    Console.WriteLine($"    {custom.Key}: {custom.Value}");
                }
            }
        }

        Console.WriteLine(new string('=', 80));
    }

    private void CompareWithBaseline(PerformanceMetrics current, PerformanceMetrics baseline)
    {
        var timeDiff = ((double)current.ElapsedMilliseconds / baseline.ElapsedMilliseconds - 1) * 100;
        var memoryDiff = ((double)current.MemoryUsedBytes / baseline.MemoryUsedBytes - 1) * 100;

        Console.WriteLine($"  ?? Comparação com Baseline:");
        
        // Tempo
        if (Math.Abs(timeDiff) < 5)
        {
            Console.WriteLine($"     Tempo: {timeDiff:+0.0;-0.0}% ? (dentro da margem)");
        }
        else if (timeDiff > 0)
        {
            Console.WriteLine($"     Tempo: {timeDiff:+0.0}% ?? REGRESSÃO!");
        }
        else
        {
            Console.WriteLine($"     Tempo: {timeDiff:+0.0}% ? MELHORIA!");
        }

        // Memória
        if (Math.Abs(memoryDiff) < 10)
        {
            Console.WriteLine($"     Memória: {memoryDiff:+0.0;-0.0}% ?");
        }
        else if (memoryDiff > 0)
        {
            Console.WriteLine($"     Memória: {memoryDiff:+0.0}% ?? AUMENTO!");
        }
        else
        {
            Console.WriteLine($"     Memória: {memoryDiff:+0.0}% ?");
        }
    }

    public bool HasRegressions(double thresholdPercent = 10.0)
    {
        bool hasRegression = false;

        foreach (var metric in _metrics)
        {
            if (_baselines.TryGetValue(metric.TestName, out var baseline))
            {
                var timeDiff = ((double)metric.ElapsedMilliseconds / baseline.ElapsedMilliseconds - 1) * 100;
                
                if (timeDiff > thresholdPercent)
                {
                    Console.WriteLine($"? REGRESSÃO DETECTADA em '{metric.TestName}': {timeDiff:+0.0}%");
                    hasRegression = true;
                }
            }
        }

        return hasRegression;
    }

    public void Clear()
    {
        _metrics.Clear();
    }
}

/// <summary>
/// Exemplo de integração com CI/CD
/// </summary>
public static class CiCdIntegrationExample
{
    public static void ShowExample()
    {
        Console.WriteLine("\n=== INTEGRAÇÃO CI/CD - EXEMPLO ===");
        Console.WriteLine("\n1. COLETA AUTOMÁTICA (em testes):");
        Console.WriteLine("```csharp");
        Console.WriteLine("[Test]");
        Console.WriteLine("public void PerformanceTest_ProcessMessages()");
        Console.WriteLine("{");
        Console.WriteLine("    using var collector = new PerformanceMetricsCollector(\"ProcessMessages\");");
        Console.WriteLine("    ");
        Console.WriteLine("    for (int i = 0; i < 10000; i++)");
        Console.WriteLine("    {");
        Console.WriteLine("        ProcessMessage();");
        Console.WriteLine("        collector.RecordOperation();");
        Console.WriteLine("    }");
        Console.WriteLine("    // Métricas automaticamente reportadas no Dispose");
        Console.WriteLine("}");
        Console.WriteLine("```");

        Console.WriteLine("\n2. SCRIPT DE CI/CD (exemplo):");
        Console.WriteLine("```yaml");
        Console.WriteLine("# .github/workflows/performance.yml");
        Console.WriteLine("- name: Run Performance Tests");
        Console.WriteLine("  run: dotnet test --filter Category=Performance");
        Console.WriteLine("");
        Console.WriteLine("- name: Check for Regressions");
        Console.WriteLine("  run: |");
        Console.WriteLine("    dotnet run --project PerfChecker -- \\");
        Console.WriteLine("      --baseline baselines.json \\");
        Console.WriteLine("      --threshold 10");
        Console.WriteLine("```");

        Console.WriteLine("\n3. FERRAMENTAS LINHA DE COMANDO:");
        Console.WriteLine("  • dotnet-trace: dotnet trace collect --process-id <PID>");
        Console.WriteLine("  • PerfView: PerfView.exe /NoGui collect /DataFile:output.etl");
        Console.WriteLine("  • BenchmarkDotNet: Framework completo para benchmarks");

        Console.WriteLine("\n4. ALERTAS E GATES:");
        Console.WriteLine("  • Falha build se regressão > 10%");
        Console.WriteLine("  • Notificação Slack/Teams com relatório");
        Console.WriteLine("  • Dashboard com histórico de métricas");
    }
}
