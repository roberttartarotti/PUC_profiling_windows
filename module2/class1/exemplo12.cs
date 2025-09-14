/*
================================================================================
ATIVIDADE PRÁTICA 12 - RESOURCE LEAKS E DISPOSE PATTERNS (C#)
================================================================================

OBJETIVO:
- Demonstrar vazamentos de recursos não-gerenciados
- Usar Memory profiler para identificar handles/recursos não liberados
- Otimizar usando using statements e Dispose patterns
- Medir impacto de resource leaks no sistema

PROBLEMA:
- Não fazer Dispose() de IDisposable causa resource leaks
- Handles de sistema não são liberados automaticamente
- Resource monitoring mostrará crescimento de handles

SOLUÇÃO:
- Usar using statements para cleanup automático
- Implementar Dispose pattern corretamente

================================================================================
*/

using System;
using System.IO;
using System.Diagnostics;

class Program {
    static void DemonstrateResourceLeaks() {
        Console.WriteLine("Starting resource leak demonstration...");
        Console.WriteLine("Monitor system resources - should see growing handle count");
        
        const int ITERATIONS = 1000;
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < ITERATIONS; i++) {
            // PERFORMANCE ISSUE: Creating FileStream without proper disposal
            var fs = new FileStream("temp.txt", FileMode.Create, FileAccess.Write);
            fs.Write(new byte[] { 1, 2, 3, 4, 5 });
            fs.Flush();
            // Missing fs.Dispose() - resource leak!
            
            // PERFORMANCE ISSUE: Creating Process without disposal
            var process = new Process();
            process.StartInfo = new ProcessStartInfo("cmd.exe", "/c echo test")
            {
                UseShellExecute = false,
                CreateNoWindow = true
            };
            // Missing process.Dispose() - handle leak!
            
            if (i % 100 == 0) {
                Console.WriteLine($"Created {i}/{ITERATIONS} undisposed resources...");
                Console.WriteLine($"Current process handles: {Process.GetCurrentProcess().HandleCount}");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Resource leak test completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Final handle count: {Process.GetCurrentProcess().HandleCount}");
        Console.WriteLine($"Resources created without disposal: {ITERATIONS * 2}");
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        Console.WriteLine($"Handle count after GC: {Process.GetCurrentProcess().HandleCount}");
    }
    
    static void Main() {
        Console.WriteLine("Starting resource management demonstration...");
        Console.WriteLine("Task: Creating system resources without proper cleanup");
        Console.WriteLine("Monitor system resources and handle counts");
        Console.WriteLine();
        
        DemonstrateResourceLeaks();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check resource monitors for:");
        Console.WriteLine("- Growing handle count");
        Console.WriteLine("- File handles not released");
        Console.WriteLine("- System resource consumption");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR A VERSÃO COM PROPER DISPOSAL)
================================================================================

using System;
using System.IO;
using System.Diagnostics;

class Program {
    static void DemonstrateProperResourceManagement() {
        Console.WriteLine("Starting proper resource management demonstration...");
        Console.WriteLine("Monitor system resources - handle count should remain stable");
        
        const int ITERATIONS = 1000;
        int initialHandles = Process.GetCurrentProcess().HandleCount;
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: Using statement ensures proper disposal
            using (var fs = new FileStream("temp.txt", FileMode.Create, FileAccess.Write)) {
                fs.Write(new byte[] { 1, 2, 3, 4, 5 });
                fs.Flush();
            } // Automatic fs.Dispose() called here
            
            // CORREÇÃO: Using statement for Process disposal
            using (var process = new Process()) {
                process.StartInfo = new ProcessStartInfo("cmd.exe", "/c echo test")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                // Process.Dispose() automatically called
            }
            
            if (i % 100 == 0) {
                Console.WriteLine($"Created {i}/{ITERATIONS} properly disposed resources...");
                Console.WriteLine($"Current process handles: {Process.GetCurrentProcess().HandleCount}");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Proper resource management completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Initial handles: {initialHandles}");
        Console.WriteLine($"Final handle count: {Process.GetCurrentProcess().HandleCount}");
        Console.WriteLine($"Handle growth: {Process.GetCurrentProcess().HandleCount - initialHandles}");
        
        // Clean up test file
        if (File.Exists("temp.txt")) {
            File.Delete("temp.txt");
        }
    }
    
    static void Main() {
        Console.WriteLine("Starting optimized resource management demonstration...");
        Console.WriteLine("Task: Creating system resources with proper cleanup");
        Console.WriteLine("Monitor system resources for stable handle usage");
        Console.WriteLine();
        
        DemonstrateProperResourceManagement();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("Improvements:");
        Console.WriteLine("- Stable handle count due to proper disposal");
        Console.WriteLine("- No resource leaks");
        Console.WriteLine("- Deterministic cleanup with using statements");
        Console.WriteLine("- Better system resource utilization");
    }
}

================================================================================
*/
