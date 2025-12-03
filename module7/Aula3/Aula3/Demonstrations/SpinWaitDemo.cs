using System.Diagnostics;
using System.Collections.Concurrent;
using Aula3.Utilities;

namespace Aula3.Demonstrations;

/// <summary>
/// Demonstra o problema de Spin Waits e seu custo oculto - VERSÃO EXTREMA
/// Slide 4: Thread esperando ativamente (em loop) consumindo CPU
/// CONFIGURADA PARA TORNAR O DESPERDÍCIO DE CPU EXTREMAMENTE VISÍVEL
/// </summary>
public static class SpinWaitDemo
{
    private static volatile bool _flag = false;
    private static volatile int _spinCounter = 0;
    private static readonly object _lock = new object();
    private static readonly ConcurrentQueue<string> _profilerHints = new();

    public static void RunDemo()
    {
        using var demoRegion = ProfilingMarkers.CreateScenarioRegion("SpinWaitDemo", 
            "Demonstração completa de problemas de spin waits vs soluções corretas");

        Console.WriteLine("\n=== DEMONSTRAÇÃO: SPIN WAITS E SEU CUSTO OCULTO ===");
        Console.WriteLine("VERSÃO EXTREMA - CONFIGURADA PARA MOSTRAR DESPERDÍCIO MASSIVO DE CPU");
        Console.WriteLine("Problema: Thread esperando ativamente em loop, consumindo CPU\n");

        ShowSpinWaitProfilingInstructions();

        // Cenário 1: BUSY WAIT EXTREMO - múltiplas threads desperdiçando CPU
        Console.WriteLine("--- Cenário 1: BUSY WAIT EXTREMO (Anti-padrão Multiplicado) ---");
        RunExtremeBusyWaitScenario();

        // Cenário 2: SpinWait com duração inadequada
        Console.WriteLine("\n--- Cenário 2: SpinWait INADEQUADO (Duração muito longa) ---");
        RunInadequateSpinWait();

        // Cenário 3: SpinLock com contenção extrema
        Console.WriteLine("\n--- Cenário 3: SpinLock com CONTENÇÃO EXTREMA ---");
        RunExtremeSpinLockContention();

        // Cenário 4: Solução correta para comparação
        Console.WriteLine("\n--- Cenário 4: Solução CORRETA (Bloqueio adequado) ---");
        RunCorrectBlocking();

        ShowSpinWaitAnalysis();
    }

    private static void ShowSpinWaitProfilingInstructions()
    {
        ProfilingMarkers.BeginScenario("SpinWaitInstructions", "Instruções de configuração para capturar spin waits no profiler");
        
        Console.WriteLine("CONFIGURAÇÃO PARA PROFILER:");
        Console.WriteLine("1. Visual Studio: Performance Profiler -> CPU Usage");
        Console.WriteLine("2. IMPORTANTE: Marque 'Show threads' para ver por thread");
        Console.WriteLine("3. Intel VTune (se disponível): 'Spin and Overhead Time' analysis");
        Console.WriteLine("4. Procure por:");
        Console.WriteLine("   - CPU 100% mas progresso zero");
        Console.WriteLine("   - Funções de spin dominando flame graph");
        Console.WriteLine("   - Threads com alta utilização sem trabalho útil");
        Console.WriteLine();
        Console.WriteLine("Pressione ENTER para iniciar cenários extremos...");
        Console.ReadKey();
        Console.WriteLine();
        
        ProfilingMarkers.EndScenario("SpinWaitInstructions");
    }

    private static void RunExtremeBusyWaitScenario()
    {
        _flag = false;
        _spinCounter = 0;
        
        int threadCount = Environment.ProcessorCount * 2;
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("ExtremeBusyWait", 
            $"Busy wait extremo com {threadCount} threads desperdiçando CPU por 20 segundos");
        
        Console.WriteLine("CRIANDO BUSY WAIT EXTREMO:");
        Console.WriteLine($"   - {threadCount} threads fazendo busy wait");
        Console.WriteLine($"   - Cada thread vai DESPERDIÇAR CPU por 20 segundos!");
        Console.WriteLine($"   - CPU usage deve ir para 100% SEM PROGRESSO");

        var stopwatch = Stopwatch.StartNew();

        // Cria múltiplas threads para busy wait - PROPOSITALMENTE EXTREMO
        var waiterTasks = new Task[threadCount];
        for (int i = 0; i < waiterTasks.Length; i++)
        {
            int threadId = i;
            waiterTasks[i] = Task.Run(() => ExtremeBusyWait(threadId));
        }

        // Monitora o desperdício em tempo real
        var monitorTask = Task.Run(() => MonitorCpuWaste(stopwatch, "ExtremeBusyWait"));

        // Deixa as threads desperdiçarem CPU por 20 segundos
        Thread.Sleep(20000);
        
        _flag = true; // Libera as threads
        Task.WaitAll(waiterTasks);
        
        stopwatch.Stop();

        ShowBusyWaitResults(stopwatch.ElapsedMilliseconds, _spinCounter, waiterTasks.Length);
    }

    private static void ExtremeBusyWait(int threadId)
    {
        var localSpinCount = 0;
        var cpuBefore = Process.GetCurrentProcess().TotalProcessorTime;

        // ANTI-PADRÃO EXTREMO: Loop vazio consumindo CPU
        while (!_flag)
        {
            localSpinCount++;
            Interlocked.Increment(ref _spinCounter);
            
            // Adiciona mais trabalho inútil para pressionar CPU
            Math.Sqrt(localSpinCount % 1000000);
        }

        var cpuAfter = Process.GetCurrentProcess().TotalProcessorTime;
        var cpuUsed = (cpuAfter - cpuBefore).TotalMilliseconds;
    }

    private static void RunInadequateSpinWait()
    {
        _flag = false;
        
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("InadequateSpinWait", 
            "SpinWait usado incorretamente para espera longa (10 segundos)");
        
        Console.WriteLine("SpinWait INADEQUADO - Muito longo para o cenário:");
        Console.WriteLine("   - SpinWait é adequado para < 1μs");
        Console.WriteLine("   - Mas vamos usar para espera de 10 segundos!");
        Console.WriteLine("   - Resultado: CPU desperdiçado por uso incorreto");

        var stopwatch = Stopwatch.StartNew();

        var inadequateSpinTask = Task.Run(() =>
        {
            var spinner = new SpinWait();
            var spinCount = 0;
            
            while (!_flag)
            {
                spinner.SpinOnce(); // Apropriado para μs, não segundos!
                spinCount++;
            }
        });

        Thread.Sleep(10000); // 10 segundos de espera inadequada
        _flag = true;
        inadequateSpinTask.Wait();
        
        stopwatch.Stop();

        Console.WriteLine($"SpinWait terminou em {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine("NO PROFILER: SpinWait.SpinOnce() deve aparecer como hotspot");
        Console.WriteLine("   - Inicial: busy spin (100% CPU)");
        Console.WriteLine("   - Depois: yields (menor CPU, mas ainda ineficiente)");
    }

    private static void RunExtremeSpinLockContention()
    {
        int threadCount = Environment.ProcessorCount * 4;
        
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("ExtremeSpinLockContention", 
            $"SpinLock com contenção extrema: {threadCount} threads com seção crítica longa");
        
        Console.WriteLine("SpinLock com CONTENÇÃO EXTREMA:");
        Console.WriteLine($"   - {threadCount} threads competindo");
        Console.WriteLine($"   - Seção crítica LONGA (inadequada para SpinLock)");
        Console.WriteLine($"   - Resultado: Threads girando desperdiçando CPU");

        var spinLock = new SpinLock(false);
        var contentionCounter = 0;
        var completedTasks = 0;

        var stopwatch = Stopwatch.StartNew();

        // PROPOSITALMENTE EXTREMO: Muitas threads + seção crítica longa
        var tasks = new Task[threadCount];
        for (int i = 0; i < tasks.Length; i++)
        {
            int taskId = i;
            tasks[i] = Task.Run(() =>
            {
                
                for (int iteration = 0; iteration < 50; iteration++) // 50 iterações por thread
                {
                    bool lockTaken = false;
                    var spinStart = Stopwatch.StartNew();
                    
                    try
                    {
                        spinLock.Enter(ref lockTaken);
                        var spinTime = spinStart.ElapsedMilliseconds;
                        
                        if (spinTime > 10) // Se esperou muito
                        {
                            Interlocked.Increment(ref contentionCounter);
                        }
                        
                        // Seção crítica LONGA (inadequada para SpinLock)
                        Thread.Sleep(50); // 50ms é MUITO longo para SpinLock!
                        
                        // Trabalho adicional na seção crítica
                        for (int work = 0; work < 100000; work++)
                        {
                            Math.Sqrt(work);
                        }
                    }
                    finally
                    {
                        if (lockTaken) spinLock.Exit();
                    }
                }
                
                Interlocked.Increment(ref completedTasks);
            });
        }

        Task.WaitAll(tasks);
        stopwatch.Stop();

        Console.WriteLine($"SpinLock extremo concluído:");
        Console.WriteLine($"   Tempo total: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"   Contenções detectadas: {contentionCounter}");
        Console.WriteLine($"   Tasks completadas: {completedTasks}");
        Console.WriteLine("NO PROFILER: SpinLock.Enter() dominando CPU time");
        Console.WriteLine("   - Threads 'spinning' enquanto aguardam lock");
        Console.WriteLine("   - CPU alto sem progresso proporcional");
    }

    private static void RunCorrectBlocking()
    {
        int threadCount = Environment.ProcessorCount * 4;
        
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("CorrectBlocking", 
            "Solução correta: mesmo trabalho com bloqueio adequado (Monitor)");
        
        Console.WriteLine("SOLUÇÃO CORRETA - Bloqueio adequado:");
        Console.WriteLine("   - Mesmo trabalho, mas com blocking correto");
        Console.WriteLine("   - CPU liberado enquanto threads aguardam");

        var correctLock = new object();
        var completedTasks = 0;

        var stopwatch = Stopwatch.StartNew();

        var tasks = new Task[threadCount];
        for (int i = 0; i < tasks.Length; i++)
        {
            int taskId = i;
            tasks[i] = Task.Run(() =>
            {
                for (int iteration = 0; iteration < 50; iteration++)
                {
                    lock (correctLock) // Monitor.Enter - suspende thread
                    {
                        // Mesma seção crítica longa
                        Thread.Sleep(50);
                        
                        for (int work = 0; work < 100000; work++)
                        {
                            Math.Sqrt(work);
                        }
                    }
                }
                
                Interlocked.Increment(ref completedTasks);
            });
        }

        Task.WaitAll(tasks);
        stopwatch.Stop();
        
        Console.WriteLine($"Bloqueio correto concluído:");
        Console.WriteLine($"   Tempo total: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"   Tasks completadas: {completedTasks}");
        Console.WriteLine("NO PROFILER: Monitor.Enter() com 'Blocked Time'");
        Console.WriteLine("   - Threads suspensas (não consumindo CPU)");
        Console.WriteLine("   - CPU disponível para outras tarefas");
    }

    private static void MonitorCpuWaste(Stopwatch mainStopwatch, string scenarioName)
    {
        using var monitorRegion = ProfilingMarkers.CreateScenarioRegion($"{scenarioName}_Monitoring", 
            "Monitoramento em tempo real do desperdício de CPU");

        var process = Process.GetCurrentProcess();
        int checkpointCount = 0;
        
        while (mainStopwatch.IsRunning && mainStopwatch.ElapsedMilliseconds < 25000)
        {
            var cpuUsage = process.TotalProcessorTime;
            
            Console.WriteLine($"[{mainStopwatch.ElapsedMilliseconds/1000}s] " +
                            $"Spins totais: {_spinCounter:N0}, " +
                            $"CPU desperdiçado: {cpuUsage.TotalMilliseconds:F0}ms");
            
            Thread.Sleep(2000);
        }
    }

    private static void ShowBusyWaitResults(long elapsedMs, int totalSpins, int threadCount)
    {
        ProfilingMarkers.BeginScenario("BusyWaitResults", "Apresentando resultados do busy wait extremo");
        
        Console.WriteLine("\nRESULTADOS DO BUSY WAIT EXTREMO:");
        Console.WriteLine($"   Threads: {threadCount}");
        Console.WriteLine($"   Tempo total: {elapsedMs}ms");
        Console.WriteLine($"   Spins desperdiçados: {totalSpins:N0}");
        Console.WriteLine($"   Spins por segundo: {(double)totalSpins / (elapsedMs / 1000.0):N0}");
        Console.WriteLine($"   Trabalho útil: 0% (ZERO!)");
        
        Console.WriteLine("\nPROCURE NO PROFILER:");
        Console.WriteLine("   - 'ExtremeBusyWait' dominando flame graph");
        Console.WriteLine("   - CPU usage ~100% por thread");
        Console.WriteLine("   - Nenhum progresso/throughput");
        Console.WriteLine("   - Intel VTune: 'Spin and Overhead Time' alto");
        
        ProfilingMarkers.EndScenario("BusyWaitResults");
    }

    private static void ShowSpinWaitAnalysis()
    {
        ProfilingMarkers.BeginScenario("SpinWaitAnalysis", "Guia completo de análise para spin waits no profiler");
        
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("GUIA DE ANÁLISE NO PROFILER - SPIN WAITS");
        Console.WriteLine(new string('=', 80));
        
        Console.WriteLine("\nSINAIS NO VISUAL STUDIO CPU USAGE:");
        Console.WriteLine("1. FLAME GRAPH:");
        Console.WriteLine("   - Busy Wait: 'ExtremeBusyWait' como função dominante");
        Console.WriteLine("   - SpinWait: 'SpinWait.SpinOnce' aparecendo frequentemente");
        Console.WriteLine("   - SpinLock: 'SpinLock.Enter' com alta exclusive time");
        
        Console.WriteLine("\n2. THREAD TIMELINE:");
        Console.WriteLine("   - Busy Wait: Threads 100% ativas (verde contínuo)");
        Console.WriteLine("   - Blocking: Threads alternando verde/vermelho");
        
        Console.WriteLine("\n3. HOT PATH:");
        Console.WriteLine("   - Mostra caminho crítico através dos spins");
        Console.WriteLine("   - Identifica onde CPU é desperdiçado");
        
        Console.WriteLine("\nSINAIS NO CONCURRENCY VISUALIZER:");
        Console.WriteLine("1. CORES AND THREADS:");
        Console.WriteLine("   - Busy Wait: Cores 100% ocupados, baixo throughput");
        Console.WriteLine("   - SpinLock: Alternância rápida entre threads");
        
        Console.WriteLine("\n2. THREAD ACTIVITY:");
        Console.WriteLine("   - Spin: Threads em 'Execution' contínuo");
        Console.WriteLine("   - Block: Threads em 'Synchronization' (vermelho)");
        
        Console.WriteLine("\nMÉTRICAS INTEL VTUNE (se disponível):");
        Console.WriteLine("1. 'Spin and Overhead Time' > 20% = Problema");
        Console.WriteLine("2. 'CPI Rate' alto durante spins");
        Console.WriteLine("3. 'Retiring' baixo (pouco trabalho útil)");
        
        Console.WriteLine("\nCOMO NAVEGAR:");
        Console.WriteLine("1. Identifique picos de CPU no timeline");
        Console.WriteLine("2. Zoom nos períodos problemáticos");
        Console.WriteLine("3. Analise Call Tree para encontrar spin functions");
        Console.WriteLine("4. Compare 'Exclusive Time' vs 'Inclusive Time'");
        Console.WriteLine("5. Use 'Filter by function' para focar em spins");
        
        ProfilingMarkers.EndScenario("SpinWaitAnalysis");
    }
}
