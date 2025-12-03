using System.Diagnostics;
using Aula3.Utilities;

namespace Aula3.Testing;

/// <summary>
/// Classe de teste para verificar se os marcadores de profiling funcionam
/// com Intel VTune, PerfView, Visual Studio Performance Profiler e Concurrency Visualizer
/// </summary>
public static class ProfilerMarkersTest
{
    public static void RunTest()
    {
        Console.WriteLine("=== TESTE DE MARCADORES DE PROFILING ===");
        Console.WriteLine("Funciona com:");
        Console.WriteLine("- Intel VTune Profiler");
        Console.WriteLine("- PerfView");
        Console.WriteLine("- Visual Studio Performance Profiler");
        Console.WriteLine("- Concurrency Visualizer");
        Console.WriteLine();
        Console.WriteLine("1. Inicie UM dos profilers acima ANTES de executar");
        Console.WriteLine("2. Procure pelos marcadores/eventos no timeline:");
        Console.WriteLine("   - TestScenario1, TestScenario2, TestScenario3");
        Console.WriteLine("   - Eventos ETW 'Aula3-Profiling'");
        Console.WriteLine("   - Métodos MarkBegin/MarkEnd no call stack");
        Console.WriteLine();
        Console.WriteLine("Pressione ENTER para iniciar o teste...");
        Console.ReadLine();
        
        // Teste usando o sistema de região automática
        TestWithRegions();
        
        // Teste usando marcadores manuais
        TestWithManualMarkers();
        
        // Teste usando marcadores nativos (VS Performance Profiler)
        TestWithNativeMarkers();
        
        Console.WriteLine("\n=== TESTE CONCLUÍDO ===");
        Console.WriteLine("Verifique no profiler se os marcadores apareceram:");
        Console.WriteLine("- Intel VTune: Procure eventos ETW 'Aula3-Profiling'");
        Console.WriteLine("- PerfView: Procure eventos ETW na timeline");
        Console.WriteLine("- VS Performance Profiler: Procure MarkBegin/MarkEnd no call tree");
        Console.WriteLine("- Concurrency Visualizer: Procure BEGIN_/END_ nos traces");
    }
    
    private static void TestWithRegions()
    {
        using (var region = ProfilingMarkers.CreateScenarioRegion("TestScenario1", "Teste com regiões automáticas"))
        {
            // Simula trabalho CPU-intensivo
            DoSomeCpuWork(1000000, "Region Test");
            
            Thread.Sleep(500); // Pausa visível no timeline
        }
    }
    
    private static void TestWithManualMarkers()
    {
        ProfilingMarkers.BeginScenario("TestScenario2", "Teste com marcadores manuais");
        
        // Simula trabalho diferente
        DoSomeMemoryWork(100000, "Manual Test");
        
        Thread.Sleep(300);
        
        ProfilingMarkers.EndScenario("TestScenario2", "Marcadores manuais concluídos");
    }
    
    private static void TestWithNativeMarkers()
    {
        NativeProfilerMarkers.MarkBegin("TestScenario3");
        
        // Simula trabalho com threading
        DoSomeThreadWork("Native Test");
        
        Thread.Sleep(200);
        
        NativeProfilerMarkers.MarkEnd("TestScenario3");
    }
    
    private static void DoSomeCpuWork(int iterations, string testName)
    {
        Console.WriteLine($"  Executando trabalho CPU: {testName}");
        double result = 0;
        for (int i = 0; i < iterations; i++)
        {
            result += Math.Sqrt(i) * Math.Sin(i) * Math.Cos(i);
        }
        Console.WriteLine($"  CPU work resultado: {result:F2}");
    }
    
    private static void DoSomeMemoryWork(int arraySize, string testName)
    {
        Console.WriteLine($"  Executando trabalho de memória: {testName}");
        var arrays = new List<double[]>();
        for (int i = 0; i < 100; i++)
        {
            var array = new double[arraySize];
            for (int j = 0; j < arraySize; j++)
            {
                array[j] = Random.Shared.NextDouble();
            }
            arrays.Add(array);
        }
        Console.WriteLine($"  Memory work: criados {arrays.Count} arrays");
        arrays.Clear(); // Força GC
    }
    
    private static void DoSomeThreadWork(string testName)
    {
        Console.WriteLine($"  Executando trabalho com threads: {testName}");
        var tasks = new List<Task>();
        
        for (int i = 0; i < 4; i++)
        {
            int taskId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100000; j++)
                {
                    Math.Sqrt(taskId + j);
                }
            }));
        }
        
        Task.WaitAll(tasks.ToArray());
        Console.WriteLine($"  Thread work concluído");
    }
}