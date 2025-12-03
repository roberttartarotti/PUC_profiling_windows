using System.Diagnostics;
using System.Runtime.InteropServices;
using Aula3.Utilities;

namespace Aula3.Demonstrations;

/// <summary>
/// Demonstra controle de Afinidade de CPU
/// Slide 7: "Prender" thread a núcleos específicos para melhorar localidade de cache
/// </summary>
public static class CpuAffinityDemo
{
    public static void RunDemo()
    {
        using var demoRegion = ProfilingMarkers.CreateScenarioRegion("CpuAffinityDemo", 
            "Demonstração de afinidade de CPU: scheduler livre vs threads pinadas");

        Console.WriteLine("\n=== DEMONSTRAÇÃO: CONTROLE DE AFINIDADE DE CPU ===");
        Console.WriteLine("Técnica: Prender threads a núcleos específicos");
        Console.WriteLine($"Núcleos Lógicos: {Environment.ProcessorCount}");
        
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("AVISO: Demonstração completa disponível apenas no Windows");
            Console.WriteLine("  Em Linux/macOS use: pthread_setaffinity_np ou taskset\n");
            
            RunConceptualDemo();
            return;
        }

        Console.WriteLine();

        // Cenário 1: Sem Afinidade (padrão)
        Console.WriteLine("--- Cenário 1: Sem Afinidade (Scheduler Livre) ---");
        RunWithoutAffinity();

        // Cenário 2: Com Afinidade
        Console.WriteLine("\n--- Cenário 2: Com Afinidade (Thread Pinada) ---");
        RunWithAffinity();

        // Cenário 3: Análise de Trade-offs
        Console.WriteLine("\n--- Cenário 3: Trade-offs da Afinidade ---");
        ShowAffinityTradeoffs();
    }

    private static void RunWithoutAffinity()
    {
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("WithoutAffinity", 
            "Execução sem afinidade: scheduler livre para mover thread entre núcleos");

        var data = GenerateTestData(10_000_000);
        var stopwatch = Stopwatch.StartNew();

        var task = Task.Run(() =>
        {
            var threadId = Environment.CurrentManagedThreadId;
            Console.WriteLine($"  Thread {threadId} iniciada (scheduler livre)");
            
            double result = ProcessData(data, "WithoutAffinity");
            
            Console.WriteLine($"  Thread {threadId} finalizada");
            return result;
        });

        task.Wait();
        stopwatch.Stop();
        
        Console.WriteLine($"Tempo: {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine("PROBLEMA: Scheduler pode mover a thread entre núcleos (perde cache)");
    }

    private static void RunWithAffinity()
    {
        using var scenarioRegion = ProfilingMarkers.CreateScenarioRegion("WithAffinity", 
            "Execução com afinidade: thread pinada ao núcleo 0");

        var data = GenerateTestData(10_000_000);
        
        try
        {
            var process = Process.GetCurrentProcess();
            var stopwatch = Stopwatch.StartNew();

            var task = Task.Run(() =>
            {
                var threadId = Environment.CurrentManagedThreadId;
                
                // Tenta pinar ao primeiro núcleo disponível
                bool affinitySet = false;
                try
                {
                    var currentThread = GetCurrentProcessThread();
                    if (currentThread != null)
                    {
                        // Pina ao núcleo 0 (primeira CPU)
                        currentThread.ProcessorAffinity = new IntPtr(1); // Máscara: 0001 = CPU 0
                        Console.WriteLine($"  Thread {threadId} pinada ao núcleo 0");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  AVISO: Não foi possível setar afinidade: {ex.Message}");
                }
                
                double result = ProcessData(data, "WithAffinity");
                
                Console.WriteLine($"  Thread {threadId} finalizada");
                return result;
            });

            task.Wait();
            stopwatch.Stop();
            
            Console.WriteLine($"Tempo: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine("BENEFÍCIO: Thread permanece no mesmo núcleo (melhor uso de cache)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERRO: {ex.Message}");
            Console.WriteLine("  Afinidade pode requerer permissões administrativas");
        }
    }

    private static void ShowAffinityTradeoffs()
    {
        ProfilingMarkers.BeginScenario("AffinityTradeoffs", "Análise de vantagens e desvantagens da afinidade de CPU");
        
        Console.WriteLine("VANTAGENS da Afinidade:");
        Console.WriteLine("  ✓ Melhor localidade de cache (L1/L2)");
        Console.WriteLine("  ✓ Desempenho previsível e consistente");
        Console.WriteLine("  ✓ Útil em sistemas NUMA (acesso memória local)");
        Console.WriteLine("  ✓ Reduz false sharing entre threads em núcleos separados");

        Console.WriteLine("\nDESVANTAGENS da Afinidade:");
        Console.WriteLine("  ✗ Pode causar desbalanceamento de carga");
        Console.WriteLine("  ✗ Perde flexibilidade do scheduler do SO");
        Console.WriteLine("  ✗ Requer conhecimento profundo do hardware");
        Console.WriteLine("  ✗ Não portável entre sistemas diferentes");

        Console.WriteLine("\nQUANDO USAR:");
        Console.WriteLine("  • Cargas de trabalho muito estáveis e previsíveis");
        Console.WriteLine("  • Processamento científico/HPC");
        Console.WriteLine("  • Sistemas real-time com requisitos estritos");
        Console.WriteLine("  • Debugging de problemas de cache");

        Console.WriteLine("\nFERRAMENTAS DE DIAGNÓSTICO:");
        Console.WriteLine("  • Windows: Process Explorer (ver afinidade)");
        Console.WriteLine("  • Linux: taskset, numactl");
        Console.WriteLine("  • Intel VTune: Análise de cache miss por core");
        
        ProfilingMarkers.EndScenario("AffinityTradeoffs");
    }

    private static void RunConceptualDemo()
    {
        ProfilingMarkers.BeginScenario("ConceptualDemo", "Demonstração conceitual para plataformas não-Windows");
        
        Console.WriteLine("--- Demonstração Conceitual ---");
        Console.WriteLine("\nCódigo exemplo (Windows):");
        Console.WriteLine("```csharp");
        Console.WriteLine("var currentThread = GetCurrentProcessThread();");
        Console.WriteLine("currentThread.ProcessorAffinity = new IntPtr(1); // CPU 0");
        Console.WriteLine("currentThread.ProcessorAffinity = new IntPtr(2); // CPU 1");
        Console.WriteLine("currentThread.ProcessorAffinity = new IntPtr(3); // CPUs 0 e 1");
        Console.WriteLine("```");
        
        Console.WriteLine("\nCódigo exemplo (Linux):");
        Console.WriteLine("```bash");
        Console.WriteLine("taskset -c 0 ./myapp     # Roda no core 0");
        Console.WriteLine("taskset -c 0-3 ./myapp   # Roda nos cores 0-3");
        Console.WriteLine("```");
        
        ProfilingMarkers.EndScenario("ConceptualDemo");
    }

    private static double[] GenerateTestData(int size)
    {
        
        var data = new double[size];
        var random = new Random(42);
        for (int i = 0; i < size; i++)
        {
            data[i] = random.NextDouble() * 100;
        }
        
        
        return data;
    }

    private static double ProcessData(double[] data, string scenario)
    {
        // Trabalho que se beneficia de cache locality
        double sum = 0;
        for (int iteration = 0; iteration < 100; iteration++)
        {
            for (int i = 0; i < data.Length; i++)
            {
                sum += Math.Sqrt(data[i]) * Math.Sin(data[i]);
            }
        }
        
        return sum;
    }

    private static ProcessThread? GetCurrentProcessThread()
    {
        try
        {
            int currentThreadId = GetCurrentThreadId();
            var process = Process.GetCurrentProcess();
            
            foreach (ProcessThread thread in process.Threads)
            {
                if (thread.Id == currentThreadId)
                {
                    return thread;
                }
            }
        }
        catch
        {
            // Ignora se não conseguir obter
        }
        return null;
    }

    [DllImport("kernel32.dll")]
    private static extern int GetCurrentThreadId();
}
