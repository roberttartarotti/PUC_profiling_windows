using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using Aula3.Utilities;

namespace Aula3.Demonstrations;

/// <summary>
/// Demonstra o problema de False Sharing - O Gargalo Fantasma - VERSÃO EXTREMA
/// Slide 5: Variáveis diferentes na mesma linha de cache causando invalidação constante
/// CONFIGURADA PARA TORNAR O CACHE PING-PONG EXTREMAMENTE VISÍVEL
/// </summary>
public static class FalseSharingDemo
{
    private const int CacheLineSize = 64; // 64 bytes típico em x86/x64
    private const int ExtremeIterations = 50_000_000; // 50M para tornar visível
    private static readonly ConcurrentQueue<string> _profilerHints = new();

    public static void RunDemo()
    {
        using var demoRegion = ProfilingMarkers.CreateScenarioRegion("FalseSharingDemo", 
            "Demonstração completa de false sharing: problemas vs soluções otimizadas");

        Console.WriteLine("\n=== DEMONSTRAÇÃO: FALSE SHARING - O GARGALO FANTASMA ===");
        Console.WriteLine("VERSÃO EXTREMA - CONFIGURADA PARA MOSTRAR CACHE LINE PING-PONG");
        Console.WriteLine($"Tamanho típico da linha de cache: {CacheLineSize} bytes");
        Console.WriteLine($"Iterações por teste: {ExtremeIterations:N0} (50 milhões!)\n");

        ShowFalseSharingProfilingInstructions();

        // Cenário 1: FALSE SHARING EXTREMO - máximo ping-pong possível
        Console.WriteLine("--- Cenário 1: FALSE SHARING EXTREMO (Cache Line Ping-Pong) ---");
        RunExtremeFalseSharingArray();

        // Cenário 2: False Sharing com estruturas
        Console.WriteLine("\n--- Cenário 2: FALSE SHARING em Estruturas (Pior Caso) ---");
        RunFalseSharingStructures();

        // Cenário 3: Solução com Padding
        Console.WriteLine("\n--- Cenário 3: SOLUÇÃO com Padding (Cache Line Separado) ---");
        RunPaddedSolution();

        // Cenário 4: Solução com ThreadLocal
        Console.WriteLine("\n--- Cenário 4: SOLUÇÃO com ThreadLocal (Zero Contenção) ---");
        RunThreadLocalSolution();

        ShowFalseSharingAnalysis();
    }

    private static void ShowFalseSharingProfilingInstructions()
    {
        ProfilingMarkers.BeginScenario("FalseSharingInstructions", "Instruções para capturar false sharing no profiler");
        
        Console.WriteLine("CONFIGURAÇÃO PARA CAPTURAR FALSE SHARING:");
        Console.WriteLine("1. Intel VTune (IDEAL): 'Memory Access' analysis");
        Console.WriteLine("   - Procure por 'LOAD_BLOCKS.STORE_FORWARD' alto");
        Console.WriteLine("   - 'False Sharing' analysis");
        Console.WriteLine("2. Visual Studio: CPU Usage + Memory Usage");
        Console.WriteLine("   - Compare performance entre cenários");
        Console.WriteLine("3. PerfView: ETW events para cache misses");
        Console.WriteLine("4. Procure por:");
        Console.WriteLine("   - Performance degradando com mais threads");
        Console.WriteLine("   - Cache miss rate alto");
        Console.WriteLine("   - Threads competindo por mesma memória");
        Console.WriteLine();
        Console.WriteLine("ATENÇÃO: Cada teste levará 30+ segundos!");
        Console.WriteLine("Pressione ENTER para iniciar...");
        Console.ReadKey();
        Console.WriteLine();
        
        ProfilingMarkers.EndScenario("FalseSharingInstructions");
    }

    private static void RunExtremeFalseSharingArray()
    {
        int threadCount = Math.Min(Environment.ProcessorCount, 8); // Limita para ter controle
        
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("ExtremeFalseSharing", 
            $"False sharing extremo: {threadCount} threads modificando array adjacente");
        
        Console.WriteLine($"FALSE SHARING EXTREMO:");
        Console.WriteLine($"   - {threadCount} threads em {threadCount} núcleos");
        Console.WriteLine($"   - Array de int[{threadCount}] - TODOS na mesma cache line!");
        Console.WriteLine($"   - {ExtremeIterations:N0} incrementos por thread");
        Console.WriteLine($"   - Esperado: CACHE LINE PING-PONG massivo");

        // Array propositalmente pequeno para forçar false sharing
        var counters = new int[threadCount]; // int = 4 bytes, 16 ints cabem em 64 bytes
        var stopwatch = Stopwatch.StartNew();

        var tasks = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int threadIndex = i;
            tasks[i] = Task.Run(() => ExtremeIncrementWork(threadIndex, counters, threadIndex));
        }

        // Monitora progresso em tempo real
        var monitorTask = Task.Run(() => MonitorFalseSharingProgress(counters, stopwatch, "ExtremeFalseSharing"));

        Task.WaitAll(tasks);
        stopwatch.Stop();

        ShowFalseSharingResults("FALSE SHARING EXTREMO", threadCount, stopwatch.ElapsedMilliseconds, 
                               counters.Sum(), counters);
        
        Console.WriteLine("\nNO PROFILER (VTune):");
        Console.WriteLine("   - Memory Access: LOAD_BLOCKS.STORE_FORWARD > 10%");
        Console.WriteLine("   - False Sharing analysis mostra contenção");
        Console.WriteLine("   - Bandwidth utilization alto com baixo throughput");
    }

    private static void RunFalseSharingStructures()
    {
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("FalseSharingStructures", 
            "False sharing em estruturas: múltiplas estruturas por cache line");
        
        Console.WriteLine("FALSE SHARING em ESTRUTURAS (Pior Caso):");
        Console.WriteLine("   - Estruturas pequenas em array");
        Console.WriteLine("   - Múltiplas estruturas por cache line");
        Console.WriteLine("   - Threads diferentes modificando estruturas adjacentes");

        int structCount = Environment.ProcessorCount;
        
        var sharedStructs = new SharedCounter[structCount];
        for (int i = 0; i < structCount; i++)
        {
            sharedStructs[i] = new SharedCounter();
        }

        var stopwatch = Stopwatch.StartNew();

        var tasks = new Task[structCount];
        for (int i = 0; i < structCount; i++)
        {
            int structIndex = i;
            tasks[i] = Task.Run(() => ExtremeStructWork(structIndex, sharedStructs[structIndex]));
        }

        Task.WaitAll(tasks);
        stopwatch.Stop();

        var totalValue = sharedStructs.Sum(s => s.Value);
        
        ShowFalseSharingResults("STRUCT FALSE SHARING", structCount, stopwatch.ElapsedMilliseconds, totalValue, null);
    }

    private static void RunPaddedSolution()
    {
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("PaddedSolution", 
            "Solução com padding: cada contador em cache line separada");
        
        Console.WriteLine("SOLUÇÃO COM PADDING:");
        Console.WriteLine("   - Cada contador em sua própria cache line (64 bytes)");
        Console.WriteLine("   - Elimina ping-pong entre núcleos");

        int threadCount = Math.Min(Environment.ProcessorCount, 8);
        
        var paddedCounters = new PaddedCounter[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            paddedCounters[i] = new PaddedCounter();
        }

        var stopwatch = Stopwatch.StartNew();

        var tasks = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int threadIndex = i;
            tasks[i] = Task.Run(() => ExtremePaddedWork(threadIndex, paddedCounters[threadIndex]));
        }

        Task.WaitAll(tasks);
        stopwatch.Stop();

        var totalValue = paddedCounters.Sum(c => c.Value);
        
        ShowFalseSharingResults("PADDED SOLUTION", threadCount, stopwatch.ElapsedMilliseconds, totalValue, null);

        Console.WriteLine($"\nTamanho de cada PaddedCounter: {Marshal.SizeOf<PaddedCounter>()} bytes");
        Console.WriteLine("NO PROFILER: Deve ser SIGNIFICATIVAMENTE mais rápido");
    }

    private static void RunThreadLocalSolution()
    {
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("ThreadLocalSolution", 
            "Solução ThreadLocal: zero contenção entre threads");
        
        Console.WriteLine("SOLUÇÃO COM ThreadLocal:");
        Console.WriteLine("   - Cada thread tem sua própria variável");
        Console.WriteLine("   - ZERO contenção de cache");

        int threadCount = Math.Min(Environment.ProcessorCount, 8);
        
        var threadLocalCounters = new ThreadLocal<int>(() => 0);
        var results = new ConcurrentBag<int>();

        var stopwatch = Stopwatch.StartNew();

        var tasks = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int threadIndex = i;
            tasks[i] = Task.Run(() =>
            {
                ExtremeThreadLocalWork(threadIndex, threadLocalCounters);
                results.Add(threadLocalCounters.Value);
            });
        }

        Task.WaitAll(tasks);
        stopwatch.Stop();

        var totalValue = results.Sum();
        
        ShowFalseSharingResults("THREADLOCAL SOLUTION", threadCount, stopwatch.ElapsedMilliseconds, totalValue, null);

        threadLocalCounters.Dispose();
    }

    private static void ExtremeIncrementWork(int threadId, int[] counters, int index)
    {

        for (int i = 0; i < ExtremeIterations; i++)
        {
            // PROPOSITALMENTE CAUSA FALSE SHARING
            counters[index]++; // Modifica posição específica do array
        }
    }

    private static void ExtremeStructWork(int threadId, SharedCounter counter)
    {
        for (int i = 0; i < ExtremeIterations; i++)
        {
            counter.Increment();
        }
    }

    private static void ExtremePaddedWork(int threadId, PaddedCounter counter)
    {
        for (int i = 0; i < ExtremeIterations; i++)
        {
            counter.Increment();
        }
    }

    private static void ExtremeThreadLocalWork(int threadId, ThreadLocal<int> threadLocalCounter)
    {
        for (int i = 0; i < ExtremeIterations; i++)
        {
            threadLocalCounter.Value++;
        }
    }

    private static void MonitorFalseSharingProgress(int[] counters, Stopwatch stopwatch, string scenarioName)
    {
        using var monitorRegion = ProfilingMarkers.CreateScenarioRegion($"{scenarioName}_Monitoring", 
            "Monitoramento em tempo real do progresso com false sharing");

        int checkpointCount = 0;
        while (stopwatch.IsRunning)
        {
            var currentSum = counters.Sum();
            var progressPercent = (double)currentSum / (ExtremeIterations * counters.Length) * 100;
            
            Console.WriteLine($"[{stopwatch.ElapsedMilliseconds/1000}s] " +
                            $"Progresso: {progressPercent:F1}% " +
                            $"Total: {currentSum:N0}");
            
            if (progressPercent >= 99.0) break;
            Thread.Sleep(3000);
        }
    }

    private static void ShowFalseSharingResults(string scenario, int threadCount, long elapsedMs, long totalValue, int[]? counters)
    {
        var throughput = totalValue / (elapsedMs / 1000.0);
        
        ProfilingMarkers.BeginScenario($"{scenario.Replace(" ", "")}_Results", $"Apresentando resultados: {scenario}");
        
        Console.WriteLine($"\nRESULTADOS {scenario}:");
        Console.WriteLine($"   Threads: {threadCount}");
        Console.WriteLine($"   Tempo: {elapsedMs:N0} ms");
        Console.WriteLine($"   Total incrementos: {totalValue:N0}");
        Console.WriteLine($"   Throughput: {throughput:N0} ops/s");
        Console.WriteLine($"   Throughput por thread: {throughput/threadCount:N0} ops/s");

        if (counters != null)
        {
            Console.WriteLine($"   Valores por thread: [{string.Join(", ", counters.Select(c => c.ToString("N0")))}]");
            Console.WriteLine($"   Distribuição de cache: {counters.Length * sizeof(int)} bytes em {Math.Ceiling((double)(counters.Length * sizeof(int)) / CacheLineSize)} cache lines");
        }
        
        ProfilingMarkers.EndScenario($"{scenario.Replace(" ", "")}_Results");
    }

    /// <summary>
    /// Contador com padding para evitar false sharing
    /// FORÇA separação de cache line completa
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 128)] // 128 bytes para garantir separação
    private struct PaddedCounter
    {
        [FieldOffset(0)]
        private int _value;

        // Padding explícito para garantir separação
        [FieldOffset(64)]
        private long _padding1;
        [FieldOffset(72)]
        private long _padding2;
        [FieldOffset(80)]
        private long _padding3;

        public int Value => _value;

        public void Increment()
        {
            _value++;
        }
    }

    /// <summary>
    /// Estrutura pequena que causa false sharing quando em array
    /// </summary>
    private class SharedCounter
    {
        private int _value = 0;
        
        public int Value => _value;
        
        public void Increment()
        {
            _value++;
        }
    }

    private static void ShowFalseSharingAnalysis()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("GUIA DE ANÁLISE - FALSE SHARING NO PROFILER");
        Console.WriteLine(new string('=', 80));
        
        Console.WriteLine("\nINTEL VTUNE (FERRAMENTA IDEAL):");
        Console.WriteLine("1. MEMORY ACCESS ANALYSIS:");
        Console.WriteLine("   - Execute: vtune -collect memory-access");
        Console.WriteLine("   - Procure: 'LOAD_BLOCKS.STORE_FORWARD' > 5%");
        Console.WriteLine("   - Métrica: 'False Sharing' diretamente reportada");
        
        Console.WriteLine("\n2. CACHE MISS ANALYSIS:");
        Console.WriteLine("   - L1 cache miss rate alto");
        Console.WriteLine("   - L2/L3 cache line transfers");
        Console.WriteLine("   - Memory bandwidth utilization vs throughput");
        
        Console.WriteLine("\nVISUAL STUDIO PROFILER:");
        Console.WriteLine("1. CPU USAGE + MEMORY USAGE:");
        Console.WriteLine("   - Compare tempos entre cenários");
        Console.WriteLine("   - False Sharing: Alto CPU, baixo throughput");
        Console.WriteLine("   - Padded: Mesmo CPU, alto throughput");
        
        Console.WriteLine("\n2. TIMELINE ANALYSIS:");
        Console.WriteLine("   - Threads com atividade sincronizada (suspeito)");
        Console.WriteLine("   - Padrão de 'stop-and-go' entre threads");
        
        Console.WriteLine("\nPERFVIEW (AVANÇADO):");
        Console.WriteLine("1. ETW EVENTS:");
        Console.WriteLine("   - Cache miss events");
        Console.WriteLine("   - Memory access patterns");
        Console.WriteLine("   - Cross-core memory transfers");
        
        Console.WriteLine("\nIDENTIFICAÇÃO NO CÓDIGO:");
        Console.WriteLine("1. ESTRUTURA DE DADOS:");
        Console.WriteLine("   - Arrays de tipos pequenos (int, bool, byte)");
        Console.WriteLine("   - Structs < 64 bytes em arrays");
        Console.WriteLine("   - Campos adjacentes modificados por threads diferentes");
        
        Console.WriteLine("\n2. PADRÃO DE ACESSO:");
        Console.WriteLine("   - Threads trabalhando em índices adjacentes");
        Console.WriteLine("   - Modificações frequentes sem sincronização explícita");
        Console.WriteLine("   - Performance piorando com mais threads (anti-scaling)");
        
        Console.WriteLine("\nSINAIS DE FALSE SHARING:");
        Console.WriteLine("• Performance PIORA com mais threads");
        Console.WriteLine("• Cache miss rate alto sem motivo aparente");
        Console.WriteLine("• Memory bandwidth alto, CPU efficiency baixa");
        Console.WriteLine("• Padrão 'saw-tooth' no timeline (stop-and-go)");
        Console.WriteLine("• Diferença dramática com padding/ThreadLocal");
        
        Console.WriteLine("\nSOLUÇÕES TESTADAS:");
        Console.WriteLine("1. PADDING: Força separação de cache line");
        Console.WriteLine("2. THREADLOCAL: Elimina compartilhamento");
        Console.WriteLine("3. ATOMIC OPERATIONS: Interlocked para coordenação");
        Console.WriteLine("4. REDESIGN: Agregar localmente, sincronizar depois");
    }
}
