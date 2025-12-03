using System.Collections.Concurrent;
using System.Diagnostics;
using Aula3.Utilities;

namespace Aula3.Demonstrations;

/// <summary>
/// Demonstra técnicas de otimização de sincronização
/// Slide 8: Reduzindo overhead além de "usar menos locks"
/// </summary>
public static class SynchronizationOptimizationDemo
{
    public static void RunDemo()
    {
        using var demoRegion = ProfilingMarkers.CreateScenarioRegion("SynchronizationOptimizationDemo", 
            "Demonstração completa de técnicas de otimização de sincronização");

        Console.WriteLine("\n=== DEMONSTRAÇÃO: OTIMIZAÇÃO DE SINCRONIZAÇÃO ===");
        Console.WriteLine("Técnica: Além de 'usar menos locks', escolher a primitiva certa\n");

        // Cenário 1: ThreadLocal vs Lock
        Console.WriteLine("--- Cenário 1: ThreadLocal<T> vs Lock ---");
        RunThreadLocalVsLock();

        // Cenário 2: ReaderWriterLockSlim (leituras frequentes)
        Console.WriteLine("\n--- Cenário 2: ReaderWriterLockSlim vs Lock ---");
        RunReaderWriterLockComparison();

        // Cenário 3: Coleções Lock-Free
        Console.WriteLine("\n--- Cenário 3: Coleções Lock-Free (Concurrent) ---");
        RunLockFreeCollections();

        // Cenário 4: Interlocked vs Lock
        Console.WriteLine("\n--- Cenário 4: Interlocked vs Lock ---");
        RunInterlockedVsLock();

        // Cenário 5: Exemplo Prático - Cache de Métricas
        Console.WriteLine("\n--- Cenário 5: Caso Prático - Sistema de Métricas ---");
        RunMetricsCacheExample();
    }

    private static void RunThreadLocalVsLock()
    {
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("ThreadLocalVsLock", 
            "Comparação de performance: ThreadLocal vs Lock para contadores");

        const int iterations = 1_000_000;
        const int threadCount = 8;

        // Com Lock
        int counterWithLock = 0;
        object lockObj = new object();
        var sw1 = Stopwatch.StartNew();

        Parallel.For(0, iterations, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, _ =>
        {
            lock (lockObj)
            {
                counterWithLock++;
            }
        });
        sw1.Stop();

        // Com ThreadLocal
        var threadLocalCounter = new ThreadLocal<int>(() => 0);
        var sw2 = Stopwatch.StartNew();

        Parallel.For(0, iterations, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, _ =>
        {
            threadLocalCounter.Value++;
        });

        // Agregação (custo único no final)
        int totalThreadLocal = 0;
        var allValues = threadLocalCounter.Values;
        foreach (var value in allValues)
        {
            totalThreadLocal += value;
        }
        sw2.Stop();

        Console.WriteLine($"Com Lock: {sw1.ElapsedMilliseconds} ms (Total: {counterWithLock:N0})");
        Console.WriteLine($"Com ThreadLocal: {sw2.ElapsedMilliseconds} ms (Total: {totalThreadLocal:N0})");
        
        double improvement = ((double)(sw1.ElapsedMilliseconds - sw2.ElapsedMilliseconds) / sw1.ElapsedMilliseconds) * 100;
        
        Console.WriteLine($"MELHORIA: {improvement:F1}% - Sem contenção durante incrementos!");
    }

    private static void RunReaderWriterLockComparison()
    {
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("ReaderWriterLockComparison", 
            "Comparação RWLock vs Lock simples em cenário read-heavy (90% leituras)");

        const int reads = 900_000;
        const int writes = 100_000; // 90% leituras, 10% escritas
        
        var data = new Dictionary<int, string>();
        for (int i = 0; i < 100; i++)
        {
            data[i] = $"Value{i}";
        }

        // Com Lock simples
        object simpleLock = new object();
        var sw1 = Stopwatch.StartNew();

        Parallel.For(0, reads + writes, i =>
        {
            if (i < reads)
            {
                // Leitura
                lock (simpleLock)
                {
                    _ = data.TryGetValue(i % 100, out _);
                }
            }
            else
            {
                // Escrita
                lock (simpleLock)
                {
                    data[i % 100] = $"Updated{i}";
                }
            }
        });
        sw1.Stop();

        // Com ReaderWriterLockSlim
        var rwLock = new ReaderWriterLockSlim();
        data.Clear();
        for (int i = 0; i < 100; i++)
        {
            data[i] = $"Value{i}";
        }

        var sw2 = Stopwatch.StartNew();

        Parallel.For(0, reads + writes, i =>
        {
            if (i < reads)
            {
                // Leitura
                rwLock.EnterReadLock();
                try
                {
                    _ = data.TryGetValue(i % 100, out _);
                }
                finally
                {
                    rwLock.ExitReadLock();
                }
            }
            else
            {
                // Escrita
                rwLock.EnterWriteLock();
                try
                {
                    data[i % 100] = $"Updated{i}";
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }
            }
        });
        sw2.Stop();

        Console.WriteLine($"Lock Simples: {sw1.ElapsedMilliseconds} ms");
        Console.WriteLine($"ReaderWriterLockSlim: {sw2.ElapsedMilliseconds} ms");
        
        double improvement = ((double)(sw1.ElapsedMilliseconds - sw2.ElapsedMilliseconds) / sw1.ElapsedMilliseconds) * 100;
        
        Console.WriteLine("BENEFÍCIO: RWLock permite múltiplas leituras simultâneas!");
        Console.WriteLine("  Ideal quando: Leituras >> Escritas (ratio > 80%)");
    }

    private static void RunLockFreeCollections()
    {
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("LockFreeCollections", 
            "Comparação Dictionary+Lock vs ConcurrentDictionary (lock-free)");

        const int operations = 500_000;

        // Dictionary com Lock
        var dictWithLock = new Dictionary<int, string>();
        object dictLock = new object();
        var sw1 = Stopwatch.StartNew();

        Parallel.For(0, operations, i =>
        {
            lock (dictLock)
            {
                dictWithLock[i] = $"Value{i}";
            }
        });
        sw1.Stop();
        
        // ConcurrentDictionary (Lock-Free)
        var concurrentDict = new ConcurrentDictionary<int, string>();
        var sw2 = Stopwatch.StartNew();

        Parallel.For(0, operations, i =>
        {
            concurrentDict[i] = $"Value{i}";
        });
        sw2.Stop();

        Console.WriteLine($"Dictionary + Lock: {sw1.ElapsedMilliseconds} ms");
        Console.WriteLine($"ConcurrentDictionary: {sw2.ElapsedMilliseconds} ms");
        
        double improvement = ((double)(sw1.ElapsedMilliseconds - sw2.ElapsedMilliseconds) / sw1.ElapsedMilliseconds) * 100;
        
        Console.WriteLine("\nColeções Concurrent disponíveis:");
        Console.WriteLine("  • ConcurrentDictionary<K,V> - Dicionário thread-safe");
        Console.WriteLine("  • ConcurrentQueue<T> - Fila FIFO lock-free");
        Console.WriteLine("  • ConcurrentBag<T> - Coleção não ordenada");
        Console.WriteLine("  • ConcurrentStack<T> - Pilha LIFO lock-free");
        Console.WriteLine("  • BlockingCollection<T> - Produtor/consumidor com bloqueio");
    }

    private static void RunInterlockedVsLock()
    {
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("InterlockedVsLock", 
            "Comparação Interlocked vs Lock para operações atômicas simples");

        const int iterations = 10_000_000;
        
        // Com Lock
        int counterLock = 0;
        object lockObj = new object();
        var sw1 = Stopwatch.StartNew();

        Parallel.For(0, iterations, _ =>
        {
            lock (lockObj)
            {
                counterLock++;
            }
        });
        sw1.Stop();

        // Com Interlocked
        int counterInterlocked = 0;
        var sw2 = Stopwatch.StartNew();

        Parallel.For(0, iterations, _ =>
        {
            Interlocked.Increment(ref counterInterlocked);
        });
        sw2.Stop();

        Console.WriteLine($"Lock: {sw1.ElapsedMilliseconds} ms (Total: {counterLock:N0})");
        Console.WriteLine($"Interlocked: {sw2.ElapsedMilliseconds} ms (Total: {counterInterlocked:N0})");
        
        double improvement = ((double)(sw1.ElapsedMilliseconds - sw2.ElapsedMilliseconds) / sw1.ElapsedMilliseconds) * 100;
        
        Console.WriteLine("VANTAGEM: Interlocked usa instruções atômicas da CPU (lock-free)");
        Console.WriteLine("  Operações: Increment, Decrement, Add, Exchange, CompareExchange");
    }

    private static void RunMetricsCacheExample()
    {
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("MetricsCacheExample", 
            "Exemplo prático: sistema de métricas otimizado com ConcurrentDictionary + Interlocked");

        Console.WriteLine("Problema: Sistema processa métricas, atualiza contadores por chave");
        Console.WriteLine("Solução: ConcurrentDictionary + Interlocked (evita lock e false sharing)\n");

        var metrics = new MetricsCache();
        const int messages = 100_000;
        var stopwatch = Stopwatch.StartNew();

        // Simula processamento paralelo de mensagens
        Parallel.For(0, messages, i =>
        {
            string metricKey = $"metric_{i % 50}"; // 50 métricas diferentes
            metrics.Increment(metricKey, 1);
        });

        stopwatch.Stop();

        var allMetrics = metrics.GetAllMetrics();
        
        Console.WriteLine($"Mensagens processadas: {messages:N0}");
        Console.WriteLine($"Tempo: {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine($"Throughput: {messages / stopwatch.Elapsed.TotalSeconds:N0} msg/s");
        Console.WriteLine($"Métricas distintas: {allMetrics.Count}");
        
        Console.WriteLine("\nArquitetura:");
        Console.WriteLine("  • ConcurrentDictionary para armazenar contadores");
        Console.WriteLine("  • Interlocked.Add para incrementos atômicos");
        Console.WriteLine("  • Sem false sharing (cada contador é objeto separado)");
        
    }
}

/// <summary>
/// Exemplo de cache de métricas otimizado para alta concorrência
/// Solução para o exercício do Slide 10
/// </summary>
public class MetricsCache
{
    private readonly ConcurrentDictionary<string, MetricCounter> _metrics = new();

    public void Increment(string key, long value)
    {
        var counter = _metrics.GetOrAdd(key, _ => new MetricCounter());
        counter.Add(value);
    }

    public long GetValue(string key)
    {
        return _metrics.TryGetValue(key, out var counter) ? counter.Value : 0;
    }

    public Dictionary<string, long> GetAllMetrics()
    {
        var snapshot = new Dictionary<string, long>();
        foreach (var kvp in _metrics)
        {
            snapshot[kvp.Key] = kvp.Value.Value;
        }
        return snapshot;
    }
}

/// <summary>
/// Contador otimizado usando Interlocked
/// Evita false sharing ao ser um objeto separado (não em array)
/// </summary>
public class MetricCounter
{
    private long _value;

    public long Value => Interlocked.Read(ref _value);

    public void Add(long delta)
    {
        Interlocked.Add(ref _value, delta);
    }
}
