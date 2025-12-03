using Aula3.Demonstrations;
using Aula3.Utilities;
using Aula3.Testing;

Console.WriteLine("╔════════════════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║   DIAGNÓSTICO PROFUNDO E OTIMIZAÇÃO AVANÇADA DE APLICAÇÕES MULTITHREAD   ║");
Console.WriteLine("║                    VERSÃO EXTREMA - LIMITES DO SISTEMA                     ║");
Console.WriteLine("║                        Módulo 7 - Parte 3                                  ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════╝");

using var applicationRegion = ProfilingMarkers.CreateScenarioRegion("Aula3Application", 
    "Aplicação completa de demonstrações de profiling extremo");

ShowProfilingSetupInstructions();

Console.WriteLine("\nDEMONSTRAÇÕES EXTREMAS (Configuradas para Explorar Limites):");
Console.WriteLine("  1. Overthreading EXTREMO - 20x mais threads que núcleos");
Console.WriteLine("  2. Spin Waits MASSIVO - CPU 100% desperdiçado por 20+ segundos");
Console.WriteLine("  3. False Sharing INTENSO - 50M operações com cache ping-pong");
Console.WriteLine("  4. CPU Affinity - Demonstração de pinning vs migration");
Console.WriteLine("  5. Synchronization - Lock contention vs Lock-free extremo");
Console.WriteLine("  6. Métricas CI/CD - Sistema de baseline e regressões");
Console.WriteLine("  7. EXECUTAR TODAS (Análise Completa - 10+ minutos)");
Console.WriteLine("  8. Ver Guia de Profiling (Como encontrar problemas)");
Console.WriteLine("  9. Configuração Rápida para Profiler");
Console.WriteLine("  T. TESTAR MARCADORES DE PROFILING (Recomendado)");
Console.WriteLine("  0. Sair");

while (true)
{
    Console.Write("\n» Escolha uma opção (0-9, T): ");
    var choice = Console.ReadLine()?.ToUpper();

    try
    {
        switch (choice)
        {
            case "1":
                ShowScenarioInstructions("OVERTHREADING EXTREMO", 
                    "Context Switches > 10,000/sec", 
                    "CPU alto, throughput baixo");
                RunWithMetrics("Overthreading Demo", OverthreadingDemo.RunDemo);
                break;

            case "2":
                ShowScenarioInstructions("SPIN WAITS MASSIVO",
                    "CPU 100% sem progresso",
                    "Spin functions dominando flame graph");
                RunWithMetrics("Spin Wait Demo", SpinWaitDemo.RunDemo);
                break;

            case "3":
                ShowScenarioInstructions("FALSE SHARING INTENSO",
                    "LOAD_BLOCKS.STORE_FORWARD > 10%",
                    "Performance piora com mais threads");
                RunWithMetrics("False Sharing Demo", FalseSharingDemo.RunDemo);
                break;

            case "4":
                ShowScenarioInstructions("CPU AFFINITY",
                    "Thread migration vs pinning",
                    "Context switches e cache locality");
                RunWithMetrics("CPU Affinity Demo", CpuAffinityDemo.RunDemo);
                break;

            case "5":
                ShowScenarioInstructions("SYNCHRONIZATION EXTREMO",
                    "Lock contention vs Lock-free",
                    "Blocked time vs Execution time");
                RunWithMetrics("Synchronization Optimization Demo", SynchronizationOptimizationDemo.RunDemo);
                break;

            case "6":
                ShowMetricsIntegration();
                break;

            case "7":
                RunAllExtremeDemos();
                break;

            case "8":
                ShowProfilingGuide();
                break;

            case "9":
                ShowQuickProfilerSetup();
                break;

            case "T":
                Console.WriteLine("\n=== TESTE DE MARCADORES DE PROFILING ===");
                Console.WriteLine("IMPORTANTE: Execute este teste PRIMEIRO para verificar se os marcadores funcionam!");
                Console.WriteLine("1. Inicie o Visual Studio Performance Profiler OU Concurrency Visualizer");
                Console.WriteLine("2. Execute este teste");
                Console.WriteLine("3. Verifique se os marcadores aparecem no timeline");
                Console.WriteLine();
                ProfilerMarkersTest.RunTest();
                break;

            case "0":
                ProfilingMarkers.BeginScenario("ApplicationExit", "Encerrando aplicação e limpando recursos");
                
                Console.WriteLine("\nEncerrando. Obrigado!");
                Console.WriteLine("\nLEMBRE-SE:");
                Console.WriteLine("   - Analise os dados coletados pelo profiler");
                Console.WriteLine("   - Compare cenários problemáticos vs otimizados");
                Console.WriteLine("   - Use o PROFILING_GUIDE.md para referência");
                
                ProfilingMarkers.EndScenario("ApplicationExit");
                ProfilingMarkers.FlushAll();
                return;

            default:
                Console.WriteLine("Opção inválida. Escolha entre 0-9 ou T.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nERRO: {ex.Message}");
        Console.WriteLine($"   Stack: {ex.StackTrace}");
        Console.WriteLine("\nSe o erro persistir, pode ser limitação do sistema devido aos cenários extremos.");
    }

    Console.WriteLine("\n" + new string('─', 80));
    Console.WriteLine("Pressione qualquer tecla para continuar...");
    Console.ReadKey();
    Console.Clear();
    ShowMenu();
}

void ShowProfilingSetupInstructions()
{
    ProfilingMarkers.BeginScenario("ProfilingSetupInstructions", "Exibindo instruções de configuração para profilers");
    
    Console.WriteLine("\nCONFIGURAÇÃO PARA CAPTURAR PROBLEMAS NO PROFILER:");
    Console.WriteLine();
    Console.WriteLine("VISUAL STUDIO PERFORMANCE PROFILER:");
    Console.WriteLine("   1. Debug → Performance Profiler (Alt+F2)");
    Console.WriteLine("   2. Marque X CPU Usage");
    Console.WriteLine("   3. Marque X Memory Usage");
    Console.WriteLine("   4. Se disponível: X Concurrency Visualizer");
    Console.WriteLine("   5. Clique Start ANTES de escolher demonstração");
    Console.WriteLine();
    Console.WriteLine("INTEL VTUNE (IDEAL para False Sharing):");
    Console.WriteLine("   1. vtune-gui (interface) ou linha de comando:");
    Console.WriteLine("   2. vtune -collect hotspots -app-args Aula3.exe");
    Console.WriteLine("   3. Para False Sharing: vtune -collect memory-access");
    Console.WriteLine();
    Console.WriteLine("IMPORTANTE: Cenários configurados para EXPLORAR LIMITES!");
    Console.WriteLine("   - Cada demonstração levará 30+ segundos");
    Console.WriteLine("   - Sistema pode ficar lento temporariamente");
    Console.WriteLine("   - PERFEITO para capturar problemas no profiler");
    
    ProfilingMarkers.EndScenario("ProfilingSetupInstructions");
}

void ShowMenu()
{
    Console.WriteLine("╔════════════════════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║              OTIMIZAÇÃO AVANÇADA - VERSÃO EXTREMA PARA PROFILING          ║");
    Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════╝");
    Console.WriteLine("\n  1. Overthreading | 2. Spin Waits | 3. False Sharing");
    Console.WriteLine("  4. CPU Affinity | 5. Sync Extremo | 6. CI/CD Metrics");
    Console.WriteLine("  7. TODAS Demos | 8. Profiling Guide | 9. Setup Rápido");
    Console.WriteLine("  T. TESTAR Marcadores | 0. Sair");
}

void ShowScenarioInstructions(string scenario, string mainMetric, string visualCue)
{
    ProfilingMarkers.BeginScenario("ScenarioInstructions", $"Preparando execução: {scenario}");
    
    Console.WriteLine($"\nPREPARANDO: {scenario}");
    Console.WriteLine($"   Métrica Principal: {mainMetric}");
    Console.WriteLine($"   Sinal Visual: {visualCue}");
    Console.WriteLine("\nSE O PROFILER ESTÁ ATIVO:");
    Console.WriteLine("   - Este cenário vai gerar dados MUITO CLAROS");
    Console.WriteLine("   - Aguarde conclusão completa para análise");
    Console.WriteLine("   - Compare com cenários otimizados");
    Console.WriteLine();
    
    ProfilingMarkers.EndScenario("ScenarioInstructions");
}

void RunWithMetrics(string testName, Action demoAction)
{
    using var metricsRegion = ProfilingMarkers.CreateScenarioRegion($"MetricsCollection_{testName.Replace(" ", "")}", 
        $"Coleta de métricas para {testName}");
    
    using var collector = new PerformanceMetricsCollector(testName);
    demoAction();
    collector.RecordOperation();
}

void RunAllExtremeDemos()
{
    using var allDemosRegion = ProfilingMarkers.CreateScenarioRegion("AllExtremeDemos", 
        "Execução sequencial de todas as demonstrações extremas");

    Console.WriteLine("\n╔════════════════════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║                    ANÁLISE COMPLETA - TODOS OS CENÁRIOS EXTREMOS           ║");
    Console.WriteLine("║                          TEMPO ESTIMADO: 10-15 MINUTOS                     ║");
    Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════╝");

    Console.WriteLine("\nESTA SEQUÊNCIA VAI GERAR DADOS COMPLETOS PARA ANÁLISE:");
    Console.WriteLine("   • Overthreading: Context switching extremo");
    Console.WriteLine("   • Spin Waits: CPU 100% desperdiçado");
    Console.WriteLine("   • False Sharing: Cache ping-pong intenso"); 
    Console.WriteLine("   • CPU Affinity: Thread migration vs pinning");
    Console.WriteLine("   • Synchronization: Contention vs Lock-free");
    Console.WriteLine();
    Console.WriteLine("CERTIFIQUE-SE:");
    Console.WriteLine("   - Profiler está ativo (Visual Studio ou VTune)");
    Console.WriteLine("   - Sistema pode ficar lento temporariamente");
    Console.WriteLine("   - Feche outros aplicativos para dados mais limpos");
    Console.WriteLine();
    Console.WriteLine("Pressione ENTER para iniciar análise completa ou ESC para cancelar...");
    
    var key = Console.ReadKey();
    if (key.Key == ConsoleKey.Escape)
    {
        Console.WriteLine("\nAnálise completa cancelada.");
        return;
    }

    Console.WriteLine("\nINICIANDO ANÁLISE COMPLETA...");

    var extremeDemos = new (string Name, Action Action, string Focus)[]
    {
        ("Overthreading Extremo", OverthreadingDemo.RunDemo, "Context Switches"),
        ("Spin Waits Massivo", SpinWaitDemo.RunDemo, "CPU Waste"),
        ("False Sharing Intenso", FalseSharingDemo.RunDemo, "Cache Ping-Pong"),
        ("CPU Affinity", CpuAffinityDemo.RunDemo, "Thread Migration"),
        ("Synchronization Extremo", SynchronizationOptimizationDemo.RunDemo, "Lock Contention")
    };

    for (int i = 0; i < extremeDemos.Length; i++)
    {
        var (name, action, focus) = extremeDemos[i];
        
        Console.WriteLine($"\n\n▼ [{i+1}/{extremeDemos.Length}] INICIANDO: {name}");
        Console.WriteLine($"    Foco: {focus}");
        Console.WriteLine(new string('─', 80));
        
        var stepTimer = System.Diagnostics.Stopwatch.StartNew();
        RunWithMetrics(name, action);
        stepTimer.Stop();
        
        Console.WriteLine(new string('─', 80));
        Console.WriteLine($"▲ CONCLUÍDO: {name} ({stepTimer.ElapsedMilliseconds/1000}s)");
        
        if (i < extremeDemos.Length - 1)
        {
            Console.WriteLine("Pausa de 3 segundos para estabilizar sistema...");
            Thread.Sleep(3000);
        }
    }

    Console.WriteLine("\nRELATÓRIO CONSOLIDADO:");

    foreach (var (name, _, focus) in extremeDemos)
    {
        Console.WriteLine($"  - {name}: Foco em '{focus}'");
    }

    Console.WriteLine("\nANÁLISE FINALIZADA. OS DADOS ESTÃO PRONTOS PARA CONSULTA NO PROFILER.");

    ProfilingMarkers.EndScenario("AllExtremeDemos");
}

void ShowMetricsIntegration()
{
    ProfilingMarkers.BeginScenario("MetricsIntegrationDemo", "Demonstração do sistema de métricas para CI/CD");
    
    Console.WriteLine("\n=== DEMONSTRAÇÃO: SISTEMA DE MÉTRICAS PARA CI/CD ===");
    Console.WriteLine("\nSimulando coleta de métricas com baseline...\n");

    // Simula um teste de performance
    var reporter = PerformanceMetricsReporter.Instance;
    reporter.Clear();

    // Define uma baseline
    var baseline = new PerformanceMetrics
    {
        TestName = "ExemploProcessamento",
        Timestamp = DateTime.UtcNow.AddDays(-1),
        ElapsedMilliseconds = 100,
        MemoryUsedBytes = 1024 * 1024, // 1MB
        ThroughputPerSecond = 10000,
        ThreadCount = 4
    };
    reporter.SetBaseline("ExemploProcessamento", baseline);

    // Executa teste atual
    Console.WriteLine("Executando teste de performance...");
    using (var collector = new PerformanceMetricsCollector("ExemploProcessamento"))
    {
        // Simula trabalho
        var random = new Random();
        double result = 0;
        for (int i = 0; i < 1_000_000; i++)
        {
            result += Math.Sqrt(random.NextDouble());
            if (i % 100_000 == 0)
                collector.RecordOperations(100_000);
        }
        
    }

    // Verifica regressões
    Console.WriteLine("\nVerificando regressões...");
    
    bool hasRegressions = reporter.HasRegressions(10.0);

    if (hasRegressions)
    {
        Console.WriteLine("\nBuild falharia no CI/CD devido a regressões!");
    }
    else
    {
        Console.WriteLine("\nBuild passaria no CI/CD - sem regressões detectadas!");
    }

    // Salva baselines
    string baselinePath = Path.Combine(Environment.CurrentDirectory, "baselines.json");
    reporter.SaveBaselinesToFile(baselinePath);
    
    Console.WriteLine("\nEM UM PIPELINE REAL:");
    Console.WriteLine("   1. Testes rodam automaticamente no PR");
    Console.WriteLine("   2. Métricas são comparadas com baseline");
    Console.WriteLine("   3. Build falha se regressão > threshold");
    Console.WriteLine("   4. Relatório é postado no PR como comentário");
    
    ProfilingMarkers.EndScenario("MetricsIntegrationDemo");
}

void ShowProfilingGuide()
{
    ProfilingMarkers.BeginScenario("ProfilingGuide", "Exibindo guia rápido de profiling");
    
    Console.WriteLine("\nGUIA RÁPIDO: COMO ENCONTRAR PROBLEMAS NO PROFILER");
    Console.WriteLine(new string('=', 70));
    
    Console.WriteLine("\nOVERTHREADING:");
    Console.WriteLine("   Visual Studio: Timeline com muitas threads simultâneas");
    Console.WriteLine("   VTune: Context Switches/sec > 10,000");
    Console.WriteLine("   Sinal: CPU alto, throughput baixo");
    
    Console.WriteLine("\nSPIN WAITS:");
    Console.WriteLine("   Visual Studio: Função spin dominando flame graph");
    Console.WriteLine("   VTune: 'Spin and Overhead Time' > 20%");
    Console.WriteLine("   Sinal: CPU 100% sem progresso");
    
    Console.WriteLine("\nFALSE SHARING:");
    Console.WriteLine("   VTune: LOAD_BLOCKS.STORE_FORWARD > 5%");
    Console.WriteLine("   Visual Studio: Performance piora com mais threads");
    Console.WriteLine("   Sinal: Timeline 'stop-and-go' pattern");
    
    Console.WriteLine("\nCPU AFFINITY:");
    Console.WriteLine("   Process Explorer: Threads migrando entre CPUs");
    Console.WriteLine("   perfmon: Context Switches/thread alto");
    Console.WriteLine("   Sinal: CPU utilization desbalanceado");
    
    Console.WriteLine("\nSYNCHRONIZATION:");
    Console.WriteLine("   Concurrency Visualizer: Blocked time alto");
    Console.WriteLine("   VTune: Lock contention > 20%");
    Console.WriteLine("   Sinal: Threads alternando exec/blocked");
    
    Console.WriteLine($"\nGUIA COMPLETO: Veja PROFILING_GUIDE.md no diretório do projeto");
    Console.WriteLine("   Contém instruções detalhadas para cada ferramenta");
    
    ProfilingMarkers.EndScenario("ProfilingGuide");
}

void ShowQuickProfilerSetup()
{
    ProfilingMarkers.BeginScenario("QuickProfilerSetup", "Exibindo configuração rápida para profiler");
    
    Console.WriteLine("\nCONFIGURAÇÃO RÁPIDA PARA PROFILER");
    Console.WriteLine(new string('=', 50));
    
    Console.WriteLine("\nVISUAL STUDIO (RECOMENDADO PARA INICIANTES):");
    Console.WriteLine("   1. Debug → Performance Profiler (Alt+F2)");
    Console.WriteLine("   2. X CPU Usage");
    Console.WriteLine("   3. X Memory Usage");
    Console.WriteLine("   4. Start → Execute Aula3.exe");
    Console.WriteLine("   5. Escolha cenário extremo");
    Console.WriteLine("   6. Aguarde conclusão e analise");
    
    Console.WriteLine("\nINTEL VTUNE (AVANÇADO - MELHOR PARA FALSE SHARING):");
    Console.WriteLine("   Comando: vtune -collect hotspots -app-args Aula3.exe");
    Console.WriteLine("   False Sharing: vtune -collect memory-access -app-args Aula3.exe");
    
    Console.WriteLine("\nWORKFLOW RECOMENDADO:");
    Console.WriteLine("   1. Configure profiler");
    Console.WriteLine("   2. Execute cenário problemático");
    Console.WriteLine("   3. Execute cenário otimizado");
    Console.WriteLine("   4. Compare resultados");
    Console.WriteLine("   5. Identifique padrões específicos");
    
    Console.WriteLine("\nDICA: Execute opção 7 (TODAS) para análise completa!");
    
    ProfilingMarkers.EndScenario("QuickProfilerSetup");
}
