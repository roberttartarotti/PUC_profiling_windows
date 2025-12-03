using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Aula3.Utilities;

namespace Aula3.Demonstrations;

/// <summary>
/// Demonstra o problema de False Sharing - O Gargalo Fantasma - VERSÃO ULTRA EXTREMA
/// Slide 5: Variáveis diferentes na mesma linha de cache causando invalidação constante
/// CONFIGURADA PARA FORÇAR CACHE PING-PONG MÁXIMO E LOAD_BLOCKS.STORE_FORWARD VISÍVEL
/// </summary>
public static class FalseSharingDemo
{
    private const int CacheLineSize = 64; // 64 bytes típico em x86/x64
    private const int UltraExtremeIterations = 1_000_000_000; // 1 BILHÃO para forçar visibilidade
    private const int IntenseLoopCount = 1000; // Loop interno intensivo
    private static readonly ConcurrentQueue<string> _profilerHints = new();

    public static void RunDemo()
    {
        using var demoRegion = ProfilingMarkers.CreateScenarioRegion("FalseSharingDemo", 
            "Demonstração ULTRA EXTREMA de false sharing: forçando LOAD_BLOCKS.STORE_FORWARD visível");

        Console.WriteLine("\n=== DEMONSTRAÇÃO: FALSE SHARING - VERSÃO ULTRA EXTREMA ===");
        Console.WriteLine("CONFIGURADA PARA FORÇAR LOAD_BLOCKS.STORE_FORWARD MASSIVO");
        Console.WriteLine($"Tamanho típico da linha de cache: {CacheLineSize} bytes");
        Console.WriteLine($"Iterações por teste: {UltraExtremeIterations:N0} (1 BILHÃO!)");
        Console.WriteLine($"Loop interno intensivo: {IntenseLoopCount} ops por iteração\n");

        ShowFalseSharingProfilingInstructions();

        // Cenário 1: FALSE SHARING ULTRA EXTREMO - máximo ping-pong possível
        Console.WriteLine("--- Cenário 1: FALSE SHARING ULTRA EXTREMO (Cache Thrashing Máximo) ---");
        RunUltraExtremeFalseSharingArray();

        // Cenário 2: Cross-Core Cache Bouncing Forçado
        Console.WriteLine("\n--- Cenário 2: CROSS-CORE CACHE BOUNCING FORÇADO ---");
        RunCrossCoreCacheBouncing();

        // Cenário 3: Write-Intensive com Memory Barriers
        Console.WriteLine("\n--- Cenário 3: WRITE-INTENSIVE + MEMORY BARRIERS ---");
        RunWriteIntensiveWithBarriers();

        // Cenário 4: Solução com Padding (para comparação)
        Console.WriteLine("\n--- Cenário 4: SOLUÇÃO com Padding (Comparação) ---");
        RunUltraPaddedSolution();

        ShowUltraFalseSharingAnalysis();
    }

    private static void ShowFalseSharingProfilingInstructions()
    {
        ProfilingMarkers.BeginScenario("FalseSharingInstructions", "Instruções para capturar false sharing extremo no profiler");
        
        Console.WriteLine("CONFIGURAÇÃO PARA CAPTURAR FALSE SHARING EXTREMO:");
        Console.WriteLine("1. Intel VTune (ESSENCIAL):");
        Console.WriteLine("   - Collection: 'Memory Access' ou 'microarchitecture'");
        Console.WriteLine("   - Procure por: 'LOAD_BLOCKS.STORE_FORWARD' > 15%");
        Console.WriteLine("   - Métrica: 'MEM_LOAD_L3_MISS_RETIRED.REMOTE_HITM'");
        Console.WriteLine("   - False Sharing: 'OFFCORE_RESPONSE.DEMAND_RFO.L3_MISS.REMOTE_HITM'");
        
        Console.WriteLine("\n2. Windows Performance Monitor (PerfMon):");
        Console.WriteLine("   - Processor\\% Processor Time (deve ser alto)");
        Console.WriteLine("   - Memory\\Cache Faults/sec");
        Console.WriteLine("   - Memory\\Page Faults/sec");
        
        Console.WriteLine("\n3. Sinais de FALSE SHARING EXTREMO esperados:");
        Console.WriteLine("   - CPU 100% mas throughput baixo");
        Console.WriteLine("   - Cache miss ratio > 50%");
        Console.WriteLine("   - Memory bandwidth saturado");
        Console.WriteLine("   - Performance PIORA dramaticamente com threads");
        
        Console.WriteLine();
        Console.WriteLine("ATENÇÃO: CADA TESTE LEVARÁ 2-5 MINUTOS!");
        Console.WriteLine("Sistema pode ficar temporariamente lento!");
        Console.WriteLine("Pressione ENTER para iniciar os testes extremos...");
        Console.ReadKey();
        Console.WriteLine();
        
        ProfilingMarkers.EndScenario("FalseSharingInstructions");
    }

    private static void RunUltraExtremeFalseSharingArray()
    {
        // Força exatamente 2 threads em núcleos diferentes para maximizar bouncing
        int threadCount = 2;
        
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("UltraExtremeFalseSharing", 
            $"False sharing ULTRA extremo: {threadCount} threads causando cache thrashing máximo");
        
        Console.WriteLine($"FALSE SHARING ULTRA EXTREMO:");
        Console.WriteLine($"   - {threadCount} threads forçadas em núcleos diferentes");
        Console.WriteLine($"   - Array de byte[2] - AMBOS na mesma cache line!");
        Console.WriteLine($"   - {UltraExtremeIterations:N0} iterações x {IntenseLoopCount} ops = {(long)UltraExtremeIterations * IntenseLoopCount:N0} operações");
        Console.WriteLine($"   - Cada thread modifica byte adjacente continuamente");
        Console.WriteLine($"   - ESPERADO: Cache line ping-pong extremo entre núcleos");

        // Array minúsculo para garantir false sharing
        var sharedBytes = new byte[2]; // Apenas 2 bytes, sempre na mesma cache line
        var stopwatch = Stopwatch.StartNew();

        var tasks = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int threadIndex = i;
            tasks[i] = Task.Run(() => UltraIntensiveWork(threadIndex, sharedBytes, threadIndex));
        }

        // Monitor com mais frequência
        var monitorTask = Task.Run(() => MonitorUltraProgress(sharedBytes, stopwatch, "UltraExtremeFalseSharing"));

        Task.WaitAll(tasks);
        stopwatch.Stop();

        var totalOps = (long)UltraExtremeIterations * IntenseLoopCount * threadCount;
        ShowUltraResults("FALSE SHARING ULTRA EXTREMO", threadCount, stopwatch.ElapsedMilliseconds, totalOps, sharedBytes);
        
        Console.WriteLine("\nEXPECTATIVAS NO PROFILER:");
        Console.WriteLine("   - VTune: LOAD_BLOCKS.STORE_FORWARD > 20%");
        Console.WriteLine("   - Memory Access: HITM events altíssimos");
        Console.WriteLine("   - CPU utilization 100% mas throughput ridículo");
        Console.WriteLine("   - Cache miss rate > 70%");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void UltraIntensiveWork(int threadId, byte[] sharedArray, int index)
    {
        var random = new Random(threadId); // Para evitar otimizações do compilador
        
        for (int i = 0; i < UltraExtremeIterations; i++)
        {
            // Loop interno intensivo para maximizar cache pressure
            for (int inner = 0; inner < IntenseLoopCount; inner++)
            {
                // Força modificação constante do mesmo byte
                sharedArray[index]++;
                
                // Força flush da cache line ocasionalmente
                if (inner % 100 == 0)
                {
                    Thread.MemoryBarrier(); // Força sincronização de memória
                }
                
                // Operação dummy para evitar otimização
                var dummy = random.Next(1, 3);
                sharedArray[index] = (byte)(sharedArray[index] + dummy - dummy);
            }
            
            // Força yield ocasional para permitir context switching
            if (i % 10000 == 0)
            {
                Thread.Yield();
            }
        }
    }

    private static void RunCrossCoreCacheBouncing()
    {
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("CrossCoreCacheBouncing", 
            "Forçando cache bouncing entre núcleos específicos");
        
        Console.WriteLine("CROSS-CORE CACHE BOUNCING FORÇADO:");
        Console.WriteLine("   - Threads alternando acesso ao mesmo endereço");
        Console.WriteLine("   - Simula pior caso de cache coherency protocol");
        Console.WriteLine("   - MESI state transitions forçadas: Modified -> Shared -> Invalid");

        var sharedCounter = new SharedAtomicCounter();
        var threadCount = Math.Min(Environment.ProcessorCount, 4);
        var reducedIterations = UltraExtremeIterations / 10; // Reduz um pouco para não travar o sistema

        var stopwatch = Stopwatch.StartNew();

        var tasks = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int threadIndex = i;
            tasks[i] = Task.Run(() => CrossCoreBounceWork(threadIndex, sharedCounter, reducedIterations));
        }

        Task.WaitAll(tasks);
        stopwatch.Stop();

        var totalOps = (long)reducedIterations * threadCount;
        Console.WriteLine($"\nRESULTADOS CROSS-CORE BOUNCING:");
        Console.WriteLine($"   Threads: {threadCount}");
        Console.WriteLine($"   Tempo: {stopwatch.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"   Total operações: {totalOps:N0}");
        Console.WriteLine($"   Valor final: {sharedCounter.Value:N0}");
        Console.WriteLine($"   Throughput: {totalOps / (stopwatch.ElapsedMilliseconds / 1000.0):N0} ops/s");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CrossCoreBounceWork(int threadId, SharedAtomicCounter counter, int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            // Força operação atômica que invalida cache em outros núcleos
            counter.Increment();
            
            // Força leitura que pode causar cache miss
            var currentValue = counter.Value;
            
            // Força write que invalida cache line em outros núcleos
            counter.Add(1);
            
            // Memory barrier ocasional para forçar sincronização
            if (i % 1000 == 0)
            {
                Thread.MemoryBarrier();
            }
        }
    }

    private static void RunWriteIntensiveWithBarriers()
    {
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("WriteIntensiveWithBarriers", 
            "Write-intensive operations com memory barriers forçados");
        
        Console.WriteLine("WRITE-INTENSIVE + MEMORY BARRIERS:");
        Console.WriteLine("   - Operações de escrita intensivas");
        Console.WriteLine("   - Memory barriers forçados");
        Console.WriteLine("   - Invalidação de cache line garantida");

        var sharedData = new IntenseWriteData();
        var threadCount = Environment.ProcessorCount;
        var reducedIterations = UltraExtremeIterations / 20;

        var stopwatch = Stopwatch.StartNew();

        var tasks = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int threadIndex = i;
            tasks[i] = Task.Run(() => IntenseWriteWork(threadIndex, sharedData, reducedIterations));
        }

        Task.WaitAll(tasks);
        stopwatch.Stop();

        var totalOps = (long)reducedIterations * threadCount * 5; // 5 operações por iteração
        Console.WriteLine($"\nRESULTADOS WRITE-INTENSIVE:");
        Console.WriteLine($"   Threads: {threadCount}");
        Console.WriteLine($"   Tempo: {stopwatch.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"   Total operações: {totalOps:N0}");
        Console.WriteLine($"   Throughput: {totalOps / (stopwatch.ElapsedMilliseconds / 1000.0):N0} ops/s");
        Console.WriteLine($"   Dados finais: A={sharedData.ValueA}, B={sharedData.ValueB}, C={sharedData.ValueC}");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void IntenseWriteWork(int threadId, IntenseWriteData data, int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            // Múltiplas escritas na mesma cache line
            data.ValueA++;
            Thread.MemoryBarrier(); // Força sincronização
            
            data.ValueB++;
            Thread.MemoryBarrier(); // Força sincronização
            
            data.ValueC++;
            Thread.MemoryBarrier(); // Força sincronização
            
            // Operações adicionais para intensificar
            data.ValueA += threadId;
            data.ValueB -= threadId;
        }
    }

    private static void RunUltraPaddedSolution()
    {
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("UltraPaddedSolution", 
            "Solução ultra padded para comparação de performance");
        
        Console.WriteLine("SOLUÇÃO ULTRA PADDED (Comparação):");
        Console.WriteLine("   - Cada valor em cache line separada");
        Console.WriteLine("   - Deve ser DRAMATICAMENTE mais rápido");

        var threadCount = 2;
        var paddedData = new UltraPaddedData[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            paddedData[i] = new UltraPaddedData();
        }

        var stopwatch = Stopwatch.StartNew();

        var tasks = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int threadIndex = i;
            tasks[i] = Task.Run(() => UltraPaddedWork(threadIndex, paddedData[threadIndex]));
        }

        Task.WaitAll(tasks);
        stopwatch.Stop();

        var totalOps = (long)UltraExtremeIterations * IntenseLoopCount * threadCount;
        var totalValue = paddedData.Sum(d => d.Value);
        
        Console.WriteLine($"\nRESULTADOS PADDED SOLUTION:");
        Console.WriteLine($"   Threads: {threadCount}");
        Console.WriteLine($"   Tempo: {stopwatch.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"   Total operações: {totalOps:N0}");
        Console.WriteLine($"   Total valor: {totalValue:N0}");
        Console.WriteLine($"   Throughput: {totalOps / (stopwatch.ElapsedMilliseconds / 1000.0):N0} ops/s");
        Console.WriteLine($"   Tamanho UltraPaddedData: {Marshal.SizeOf<UltraPaddedData>()} bytes");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void UltraPaddedWork(int threadId, UltraPaddedData data)
    {
        for (int i = 0; i < UltraExtremeIterations; i++)
        {
            for (int inner = 0; inner < IntenseLoopCount; inner++)
            {
                data.Increment();
            }
        }
    }

    private static void MonitorUltraProgress(byte[] sharedData, Stopwatch stopwatch, string scenarioName)
    {
        using var monitorRegion = ProfilingMarkers.CreateScenarioRegion($"{scenarioName}_Monitoring", 
            "Monitoramento ultra intensivo do progresso");

        while (stopwatch.IsRunning)
        {
            var dataSum = sharedData.Sum(b => (int)b);
            
            Console.WriteLine($"[{stopwatch.ElapsedMilliseconds/1000}s] " +
                            $"Dados compartilhados: [{sharedData[0]}, {sharedData[1]}] " +
                            $"Soma: {dataSum}");
            
            Thread.Sleep(2000); // Check mais frequente
        }
    }

    private static void ShowUltraResults(string scenario, int threadCount, long elapsedMs, long totalOps, byte[] data)
    {
        var throughput = totalOps / Math.Max(elapsedMs / 1000.0, 0.001);
        
        ProfilingMarkers.BeginScenario($"{scenario.Replace(" ", "")}_Results", $"Resultados ultra: {scenario}");
        
        Console.WriteLine($"\nRESULTADOS {scenario}:");
        Console.WriteLine($"   Threads: {threadCount}");
        Console.WriteLine($"   Tempo TOTAL: {elapsedMs:N0} ms ({elapsedMs/1000.0:F1}s)");
        Console.WriteLine($"   Total operações: {totalOps:N0}");
        Console.WriteLine($"   Throughput: {throughput:N0} ops/s");
        Console.WriteLine($"   Throughput por thread: {throughput/threadCount:N0} ops/s");
        Console.WriteLine($"   Dados finais: [{string.Join(", ", data)}]");
        Console.WriteLine($"   Cache lines ocupadas: {Math.Ceiling((double)data.Length / CacheLineSize)} (TODAS compartilhadas!)");
        
        ProfilingMarkers.EndScenario($"{scenario.Replace(" ", "")}_Results");
    }

    /// <summary>
    /// Estrutura para operações atômicas que causam cache bouncing
    /// </summary>
    private class SharedAtomicCounter
    {
        private volatile int _value = 0;
        
        public int Value => _value;
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Increment()
        {
            Interlocked.Increment(ref _value);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Add(int value)
        {
            Interlocked.Add(ref _value, value);
        }
    }

    /// <summary>
    /// Dados para write-intensive operations
    /// </summary>
    private class IntenseWriteData
    {
        public volatile int ValueA = 0;
        public volatile int ValueB = 0;  
        public volatile int ValueC = 0;
        // Propositalmente na mesma cache line para false sharing
    }

    /// <summary>
    /// Dados com padding ultra agressivo
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 256)] // 4 cache lines de separação
    private struct UltraPaddedData
    {
        [FieldOffset(0)]
        private long _value;
        
        // Padding massivo
        [FieldOffset(64)]  private long _pad1;
        [FieldOffset(128)] private long _pad2;
        [FieldOffset(192)] private long _pad3;

        public long Value => _value;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Increment()
        {
            _value++;
        }
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

    private static void ShowUltraFalseSharingAnalysis()
    {
        Console.WriteLine("\n" + new string('=', 90));
        Console.WriteLine("ANÁLISE ULTRA EXTREMA - FALSE SHARING MÁXIMO");
        Console.WriteLine(new string('=', 90));
        
        Console.WriteLine("\nCONFIGURAÇÃO PARA CAPTURAR LOAD_BLOCKS.STORE_FORWARD:");
        Console.WriteLine("\n1. INTEL VTUNE (OBRIGATÓRIO para métricas precisas):");
        Console.WriteLine("   Comando: vtune -collect memory-access -result-dir vtune_results ./Aula3.exe");
        Console.WriteLine("   MÉTRICAS CHAVE:");
        Console.WriteLine("   • LOAD_BLOCKS.STORE_FORWARD: > 20% (CRÍTICO)");
        Console.WriteLine("   • MEM_LOAD_L3_MISS_RETIRED.REMOTE_HITM: > 15%");
        Console.WriteLine("   • OFFCORE_RESPONSE.DEMAND_RFO.L3_MISS.REMOTE_HITM: > 10%");
        Console.WriteLine("   • Memory Bandwidth Utilization: > 80% com baixo throughput");
        
        Console.WriteLine("\n2. PERFORMANCE COUNTERS (Windows):");
        Console.WriteLine("   • Processor(_Total)\\% Processor Time: ~100%");
        Console.WriteLine("   • Memory\\Cache Faults/sec: > 10,000");
        Console.WriteLine("   • Memory\\Pages/sec: > 1,000");
        Console.WriteLine("   • System\\Context Switches/sec: muito alto");
        
        Console.WriteLine("\n3. VISUAL STUDIO DIAGNOSTIC TOOLS:");
        Console.WriteLine("   • CPU Usage: 100% mas throughput baixíssimo");
        Console.WriteLine("   • Memory Usage: padrão de acesso errático");
        Console.WriteLine("   • Timeline: threads em constante contenção");
        
        Console.WriteLine("\nSINAIS INEQUÍVOCOS DE FALSE SHARING EXTREMO:");
        Console.WriteLine("• ??  Performance DESMORONA com threads (anti-scaling)");
        Console.WriteLine("• ??  Cache miss ratio > 50% sem motivo aparente");
        Console.WriteLine("• ??  CPU 100% mas operações/segundo ridículas");
        Console.WriteLine("• ??  Memory bandwidth saturado (>80% utilization)");
        Console.WriteLine("• ??  LOAD_BLOCKS.STORE_FORWARD > 20% (smoking gun!)");
        Console.WriteLine("• ??  Diferença DRAMÁTICA com versão padded (10x-100x)");
        
        Console.WriteLine("\nINTERPRETAÇÃO DOS RESULTADOS:");
        Console.WriteLine("1. CENÁRIO FALSE SHARING:");
        Console.WriteLine("   - Deve ser o MAIS LENTO de todos");
        Console.WriteLine("   - Throughput/thread diminui drasticamente");
        Console.WriteLine("   - CPU busy mas pouco trabalho útil");
        
        Console.WriteLine("\n2. CENÁRIO PADDED:");
        Console.WriteLine("   - Deve ser 10x-100x MAIS RÁPIDO");
        Console.WriteLine("   - Throughput/thread linear ou quase");
        Console.WriteLine("   - CPU eficiente, alto trabalho útil");
        
        Console.WriteLine("\n3. DIFERENÇA ESPERADA:");
        Console.WriteLine("   - False Sharing: ~1M-10M ops/s");
        Console.WriteLine("   - Padded Solution: ~100M-1B ops/s");
        Console.WriteLine("   - Ratio: 10x-100x improvement");
        
        Console.WriteLine("\nCOMANDOS VTUNE ESPECÍFICOS:");
        Console.WriteLine("# Coleta geral de memory access");
        Console.WriteLine("vtune -collect memory-access -knob sampling-interval=1 \\");
        Console.WriteLine("       -result-dir vtune_false_sharing ./Aula3.exe");
        
        Console.WriteLine("\n# Análise específica de false sharing"); 
        Console.WriteLine("vtune -collect memory-consumption -knob analyze-mem-objects=true \\");
        Console.WriteLine("       -result-dir vtune_memory ./Aula3.exe");
        
        Console.WriteLine("\n# Report das métricas críticas");
        Console.WriteLine("vtune -report summary -result-dir vtune_false_sharing");
        Console.WriteLine("vtune -report hotspots -group-by=function -result-dir vtune_false_sharing");
    }
}
