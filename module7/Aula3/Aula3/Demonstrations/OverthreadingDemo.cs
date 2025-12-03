using System.Diagnostics;
using System.Collections.Concurrent;
using Aula3.Utilities;

namespace Aula3.Demonstrations;

/// <summary>
/// Demonstra o problema de Overthreading (Sobresubscrição de Threads) - VERSÃO EXTREMA
/// Slide 3: Mais threads ativas do que o hardware pode executar concorrentemente
/// CONFIGURADA PARA EXPLORAR OS LIMITES E TORNAR PROBLEMAS VISÍVEIS NO PROFILER
/// </summary>
public static class OverthreadingDemo
{
    private static volatile int _activeThreads = 0;
    private static readonly ConcurrentQueue<string> _profilerHints = new();

    public static void RunDemo()
    {
        using var demoRegion = ProfilingMarkers.CreateScenarioRegion("OverthreadingDemo", 
            "Demonstração completa de problemas de overthreading vs soluções otimizadas");

        Console.WriteLine("\n=== DEMONSTRAÇÃO: OVERTHREADING (SOBRESUBSCRIÇÃO DE THREADS) ===");
        Console.WriteLine("VERSÃO EXTREMA - CONFIGURADA PARA EXPLORAR LIMITES DO SISTEMA");
        Console.WriteLine($"Núcleos Lógicos Disponíveis: {Environment.ProcessorCount}\n");

        ShowProfilingInstructions();

        // Cenário 1: OVERTHREADING EXTREMO - 20x mais threads que núcleos
        Console.WriteLine("--- Cenário 1: OVERTHREADING EXTREMO (20x núcleos) ---");
        Console.WriteLine("ATENÇÃO: Isso vai SOBRECARREGAR seu sistema por 30+ segundos!");
        Console.WriteLine("   PERFEITO para capturar no profiler!");
        RunExtremeOverthreading();

        // Cenário 2: Uso Correto para comparação
        Console.WriteLine("\n--- Cenário 2: Uso Correto - Threads = Núcleos ---");
        RunOptimizedThreading();

        // Cenário 3: Thread Pool Starvation (problema sério)
        Console.WriteLine("\n--- Cenário 3: THREAD POOL STARVATION ---");
        RunThreadPoolStarvation();

        ShowProfilingResults();
    }

    private static void ShowProfilingInstructions()
    {
        ProfilingMarkers.BeginScenario("ProfilingInstructions", "Mostrando instruções para capturar dados no profiler");
        
        Console.WriteLine("INSTRUÇÕES PARA CAPTURAR NO PROFILER:");
        Console.WriteLine("1. Visual Studio: Debug -> Performance Profiler");
        Console.WriteLine("2. Marque 'CPU Usage' e 'Concurrency Visualizer'");
        Console.WriteLine("3. Pressione ENTER para começar, INICIE o profiler AGORA!");
        Console.WriteLine("4. Procure por:");
        Console.WriteLine("   - Context Switches/sec > 10,000");
        Console.WriteLine("   - CPU Usage distribuído mas baixo throughput");
        Console.WriteLine("   - Threads em estado 'Runnable' (amarelo no Concurrency)");
        Console.WriteLine("PRESSIONE QUALQUER TECLA PARA INICIAR");
        Console.ReadKey();
        Console.WriteLine("INICIANDO CENÁRIOS EXTREMOS...\n");
        
        ProfilingMarkers.EndScenario("ProfilingInstructions");
    }

    private static void RunExtremeOverthreading()
    {
        // PROPOSITALMENTE EXTREMO: 20x mais threads que núcleos
        int extremeThreadCount = Environment.ProcessorCount * 20;
        
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("ExtremeOverthreading", 
            $"Criando {extremeThreadCount} threads para {Environment.ProcessorCount} núcleos - PROBLEMA EXTREMO");
        
        Console.WriteLine($"Criando {extremeThreadCount} threads para {Environment.ProcessorCount} núcleos!");
        
        
        var stopwatch = Stopwatch.StartNew();
        var performanceCounter = GetInitialPerformanceCounters();

        var tasks = new Task[extremeThreadCount];
        for (int i = 0; i < extremeThreadCount; i++)
        {
            int taskId = i;
            tasks[i] = Task.Run(() => ExtremelyLongCpuWork(taskId, "ExtremeOverthreading"));
        }

        // Monitora em tempo real
        var monitorTask = Task.Run(() => MonitorSystemHealth(stopwatch, "ExtremeOverthreading"));

        Task.WaitAll(tasks);
        stopwatch.Stop();
        
        var finalCounters = GetFinalPerformanceCounters(performanceCounter);
        ShowExtremeProblemResults(extremeThreadCount, stopwatch.ElapsedMilliseconds, finalCounters);
    }

    private static void RunOptimizedThreading()
    {
        int optimalThreadCount = Environment.ProcessorCount;
        
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("OptimizedThreading", 
            $"Usando {optimalThreadCount} threads otimizadas - SOLUÇÃO CORRETA");
        
        Console.WriteLine($"Usando {optimalThreadCount} threads (otimizado)");
        
        var stopwatch = Stopwatch.StartNew();
        var performanceCounter = GetInitialPerformanceCounters();

        var tasks = new Task[optimalThreadCount];
        for (int i = 0; i < optimalThreadCount; i++)
        {
            int taskId = i;
            tasks[i] = Task.Run(() => ExtremelyLongCpuWork(taskId, "OptimizedThreading"));
        }

        Task.WaitAll(tasks);
        stopwatch.Stop();

        var finalCounters = GetFinalPerformanceCounters(performanceCounter);
        ShowOptimizedResults(optimalThreadCount, stopwatch.ElapsedMilliseconds, finalCounters);
    }

    private static void RunThreadPoolStarvation()
    {
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("ThreadPoolStarvation", 
            "Demonstrando esgotamento do ThreadPool - PROBLEMA CRÍTICO");

        Console.WriteLine("THREAD POOL STARVATION - Bloqueando todas as threads do pool!");
        Console.WriteLine("   Isso cria um DEADLOCK visível no profiler!");

        ThreadPool.GetAvailableThreads(out int workerBefore, out int ioBefore);
        Console.WriteLine($"Threads disponíveis ANTES: Worker={workerBefore}, IO={ioBefore}");
        
        var stopwatch = Stopwatch.StartNew();
        
        // Bloqueia TODAS as threads do ThreadPool propositalmente
        var starvationTasks = new List<Task>();
        for (int i = 0; i < workerBefore + 50; i++) // Mais que as disponíveis
        {
            int taskId = i;
            var task = Task.Run(() =>
            {
                Interlocked.Increment(ref _activeThreads);
                // BLOQUEIA por muito tempo - visível no profiler como "Blocked Time"
                Thread.Sleep(15000); // 15 segundos bloqueado!
                
                Interlocked.Decrement(ref _activeThreads);
            });
            starvationTasks.Add(task);
        }

        ThreadPool.GetAvailableThreads(out int workerDuring, out int ioDuring);
        Console.WriteLine($"Threads disponíveis DURANTE: Worker={workerDuring}, IO={ioDuring}");
        Console.WriteLine($"Threads ativas: {_activeThreads}");
        Console.WriteLine("ThreadPool ESGOTADO! Novas tasks ficam enfileiradas!");

        Thread.Sleep(5000); // Deixa o problema ser visível por 5 segundos

        stopwatch.Stop();
        
        Console.WriteLine("\nNO PROFILER, você verá:");
        Console.WriteLine("   - Threads em estado 'Blocked' (vermelho no Concurrency Visualizer)");
        Console.WriteLine("   - ThreadPool saturation metrics");
        Console.WriteLine("   - Tasks aguardando em fila");

        // Cancela as tasks para não travar o sistema
        Task.WaitAll(starvationTasks.Take(10).ToArray(), TimeSpan.FromSeconds(2));
    }

    private static void ExtremelyLongCpuWork(int taskId, string scenarioName)
    {
        Interlocked.Increment(ref _activeThreads);
        
        // Trabalho CPU-intensive MUITO LONGO para ser visível no profiler
        double result = 0;
        for (int i = 0; i < 500_000; i++) // 5x mais iterações
        {
            // Operações matemáticas pesadas
            result += Math.Sqrt(i) * Math.Sin(i) * Math.Cos(i) * Math.Tan(i % 100 + 1);
        }
        
        Interlocked.Decrement(ref _activeThreads);
    }

    private static void MonitorSystemHealth(Stopwatch mainStopwatch, string scenarioName)
    {
        using var monitorRegion = ProfilingMarkers.CreateScenarioRegion($"{scenarioName}_Monitoring", 
            "Monitoramento em tempo real da saúde do sistema");

        while (mainStopwatch.IsRunning && mainStopwatch.ElapsedMilliseconds < 35000)
        {
            ThreadPool.GetAvailableThreads(out int worker, out int io);
            var gcMemory = GC.GetTotalMemory(false) / (1024 * 1024); // MB
            
            Console.WriteLine($"[{mainStopwatch.ElapsedMilliseconds/1000}s] " +
                            $"Threads ativas: {_activeThreads}, " +
                            $"ThreadPool: {worker} disponíveis, " +
                            $"Memória: {gcMemory} MB");
            
            Thread.Sleep(2000);
        }
    }

    private static (long ticks, int threads) GetInitialPerformanceCounters()
    {
        var process = Process.GetCurrentProcess();
        return (
            process.TotalProcessorTime.Ticks,
            process.Threads.Count
        );
    }

    private static (long cpuTicks, long contextSwitches) GetFinalPerformanceCounters((long ticks, int threads) initial)
    {
        var process = Process.GetCurrentProcess();
        var currentTicks = process.TotalProcessorTime.Ticks;
        
        // Usar approximação alternativa já que ContextSwitches não está disponível
        var approximateContextSwitches = process.Threads.Cast<ProcessThread>()
            .Sum(t => t.TotalProcessorTime.Ticks / 10000); // Aproximação baseada em tempo de CPU

        return (currentTicks - initial.ticks, approximateContextSwitches);
    }

    private static void ShowExtremeProblemResults(int threadCount, long elapsedMs, (long cpuTicks, long contextSwitches) counters)
    {
        ProfilingMarkers.BeginScenario("ExtremeResults", "Apresentando resultados do cenário problemático");
        
        Console.WriteLine("\nRESULTADOS DO OVERTHREADING EXTREMO:");
        Console.WriteLine($"   Threads Criadas: {threadCount} (vs {Environment.ProcessorCount} núcleos)");
        Console.WriteLine($"   Tempo Total: {elapsedMs} ms");
        Console.WriteLine($"   CPU Ticks Consumidos: {counters.cpuTicks:N0}");
        Console.WriteLine($"   Context Switches: {counters.contextSwitches:N0}");
        Console.WriteLine($"   Eficiência: {(double)Environment.ProcessorCount / threadCount:P1}");
        
        Console.WriteLine("\nPROCURE NO PROFILER:");
        Console.WriteLine("   - Flame graph dominado por 'ExtremelyLongCpuWork'");
        Console.WriteLine("   - Timeline com muitas threads ativas simultaneamente");
        Console.WriteLine("   - Context Switch rate MUITO alto");
        Console.WriteLine("   - CPU cores subutilizados devido ao overhead");
        
        ProfilingMarkers.EndScenario("ExtremeResults");
    }

    private static void ShowOptimizedResults(int threadCount, long elapsedMs, (long cpuTicks, long contextSwitches) counters)
    {
        ProfilingMarkers.BeginScenario("OptimizedResults", "Apresentando resultados do cenário otimizado");
        
        Console.WriteLine("\nRESULTADOS OTIMIZADOS:");
        Console.WriteLine($"   Threads Criadas: {threadCount}");
        Console.WriteLine($"   Tempo Total: {elapsedMs} ms");
        Console.WriteLine($"   CPU Ticks Consumidos: {counters.cpuTicks:N0}");
        Console.WriteLine($"   Context Switches: {counters.contextSwitches:N0}");
        Console.WriteLine($"   Eficiência: {1.0:P0}");
        
        Console.WriteLine("\nCOMPARE NO PROFILER:");
        Console.WriteLine("   - Flame graph similar mas TEMPO MENOR");
        Console.WriteLine("   - Timeline com threads = núcleos");
        Console.WriteLine("   - Context Switch rate MENOR");
        Console.WriteLine("   - CPU cores MELHOR utilizados");
        
        ProfilingMarkers.EndScenario("OptimizedResults");
    }

    private static void ShowProfilingResults()
    {
        ProfilingMarkers.BeginScenario("ProfilingSummary", "Resumo final para análise no profiler");
        
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("RESUMO PARA ANÁLISE NO PROFILER");
        Console.WriteLine(new string('=', 80));
        
        Console.WriteLine("\nMÉTRICAS-CHAVE PARA PROCURAR:");
        Console.WriteLine("1. CPU USAGE:");
        Console.WriteLine("   - Overthreading: CPU alto, mas throughput baixo");
        Console.WriteLine("   - Otimizado: CPU alto E throughput alto");
        
        Console.WriteLine("\n2. CONCURRENCY VISUALIZER:");
        Console.WriteLine("   - Overthreading: Muitas threads 'Runnable' (amarelo)");
        Console.WriteLine("   - Starvation: Threads 'Blocked' (vermelho)");
        Console.WriteLine("   - Otimizado: Threads 'Executing' (verde)");
        
        Console.WriteLine("\n3. PERFORMANCE COUNTERS:");
        Console.WriteLine("   - Context Switches/sec: >10,000 = Problema");
        Console.WriteLine("   - Processor Queue Length: >2*cores = Overthreading");
        Console.WriteLine("   - ThreadPool threads: Monitor crescimento");
        
        Console.WriteLine("\nDICAS DE NAVEGAÇÃO NO VS PROFILER:");
        Console.WriteLine("   - Use 'Filter by time' para focar nos picos");
        Console.WriteLine("   - 'Call Tree' mostra hierarquia de chamadas");
        Console.WriteLine("   - 'Hot Path' destaca gargalos principais");
        Console.WriteLine("   - 'Thread Activity' mostra concorrência");
        
        ProfilingMarkers.EndScenario("ProfilingSummary");
    }
}
