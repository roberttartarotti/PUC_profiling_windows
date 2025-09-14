/*
================================================================================
ATIVIDADE PRÁTICA 15 - LINQ PERFORMANCE ANTI-PATTERNS (C#)
================================================================================

OBJETIVO:
- Demonstrar anti-patterns de LINQ que causam performance issues
- Usar CPU profiler para identificar multiple enumeration overhead
- Otimizar usando ToList(), caching, e LINQ eficiente
- Medir impacto de multiple enumerations

PROBLEMA:
- Multiple enumeration de IEnumerable causa re-execução custosa
- Chains complexas de LINQ sem materialização são ineficientes
- CPU Profiler mostrará tempo gasto em repeated enumerations

SOLUÇÃO:
- Materializar results com ToList() quando necessário
- Evitar repeated enumerations

================================================================================
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

class Program {
    static void DemonstrateLinqAntiPatterns() {
        Console.WriteLine("Starting LINQ anti-patterns demonstration...");
        Console.WriteLine("Monitor CPU profiler - should see repeated enumeration overhead");
        
        const int DATA_SIZE = 100000;
        
        // Create test data
        var numbers = Enumerable.Range(1, DATA_SIZE);
        
        var sw = Stopwatch.StartNew();
        
        // PERFORMANCE ISSUE: Multiple enumeration of expensive LINQ chain
        var expensiveQuery = numbers
            .Where(x => x % 2 == 0)              // Filter even numbers
            .Select(x => x * x)                  // Square them
            .Where(x => x > 1000)                // Filter large squares
            .Select(x => Math.Sqrt(x));          // Expensive sqrt operation
        
        // PERFORMANCE ISSUE: Each of these operations re-executes the entire chain
        int count = expensiveQuery.Count();              // Enumeration #1
        double sum = expensiveQuery.Sum();               // Enumeration #2  
        double average = expensiveQuery.Average();       // Enumeration #3
        double max = expensiveQuery.Max();               // Enumeration #4
        double min = expensiveQuery.Min();               // Enumeration #5
        
        // PERFORMANCE ISSUE: More re-enumeration
        var sortedResults = expensiveQuery.OrderBy(x => x).Take(10); // Enumeration #6
        var topResults = sortedResults.ToArray();                    // Enumeration #7
        
        sw.Stop();
        
        Console.WriteLine($"LINQ anti-patterns completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Count: {count}, Sum: {sum:F2}, Avg: {average:F2}");
        Console.WriteLine($"Max: {max:F2}, Min: {min:F2}");
        Console.WriteLine($"Top 10 results: {string.Join(", ", topResults.Take(5).Select(x => x.ToString("F1")))}...");
        Console.WriteLine($"Expensive query enumerated ~7 times (massive overhead!)");
    }
    
    static void Main() {
        Console.WriteLine("Starting LINQ performance demonstration...");
        Console.WriteLine("Task: Processing data with inefficient LINQ patterns");
        Console.WriteLine("Monitor CPU Usage Tool for repeated enumeration overhead");
        Console.WriteLine();
        
        DemonstrateLinqAntiPatterns();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check CPU profiler for:");
        Console.WriteLine("- Repeated execution of LINQ operations");
        Console.WriteLine("- Multiple enumerations of the same query");
        Console.WriteLine("- Wasted CPU cycles on redundant calculations");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR LINQ EFICIENTE)
================================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

class Program {
    static void DemonstrateEfficientLinq() {
        Console.WriteLine("Starting efficient LINQ demonstration...");
        Console.WriteLine("Monitor CPU profiler - should see single enumeration");
        
        const int DATA_SIZE = 100000;
        
        // Create test data
        var numbers = Enumerable.Range(1, DATA_SIZE);
        
        var sw = Stopwatch.StartNew();
        
        // CORREÇÃO: Materialize expensive query once with ToList()
        var expensiveQuery = numbers
            .Where(x => x % 2 == 0)              // Filter even numbers
            .Select(x => x * x)                  // Square them  
            .Where(x => x > 1000)                // Filter large squares
            .Select(x => Math.Sqrt(x))           // Expensive sqrt operation
            .ToList();                           // CORREÇÃO: Materialize once!
        
        // CORREÇÃO: Now all operations work on materialized list - no re-enumeration
        int count = expensiveQuery.Count;                    // O(1) - uses Count property
        double sum = expensiveQuery.Sum();                   // Single enumeration
        double average = expensiveQuery.Average();           // Single enumeration
        double max = expensiveQuery.Max();                   // Single enumeration
        double min = expensiveQuery.Min();                   // Single enumeration
        
        // CORREÇÃO: Working with materialized data
        var sortedResults = expensiveQuery.OrderBy(x => x).Take(10).ToArray(); // Single sort + take
        
        sw.Stop();
        
        Console.WriteLine($"Efficient LINQ completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Count: {count}, Sum: {sum:F2}, Avg: {average:F2}");
        Console.WriteLine($"Max: {max:F2}, Min: {min:F2}");
        Console.WriteLine($"Top 10 results: {string.Join(", ", sortedResults.Take(5).Select(x => x.ToString("F1")))}...");
        Console.WriteLine($"Expensive query enumerated only ONCE (optimized!)");
    }
    
    static void DemonstrateStreamingLinq() {
        Console.WriteLine("Starting streaming LINQ for large datasets...");
        
        const int DATA_SIZE = 1000000; // Larger dataset
        
        var sw = Stopwatch.StartNew();
        
        // CORREÇÃO: For very large datasets, use streaming approach
        var result = Enumerable.Range(1, DATA_SIZE)
            .Where(x => x % 1000 == 0)           // Early filtering
            .Select(x => x * 2)                  // Simple transform
            .Take(100)                           // CORREÇÃO: Take early to limit processing
            .ToArray();                          // Materialize only final small result
        
        sw.Stop();
        
        Console.WriteLine($"Streaming LINQ completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Processed {DATA_SIZE:N0} items, returned {result.Length} results");
        Console.WriteLine($"Early filtering and Take() prevented unnecessary processing");
    }
    
    static void Main() {
        Console.WriteLine("Starting optimized LINQ demonstration...");
        Console.WriteLine("Task: Processing data with efficient LINQ patterns");
        Console.WriteLine("Monitor CPU Usage Tool for improved performance");
        Console.WriteLine();
        
        DemonstrateEfficientLinq();
        Console.WriteLine();
        DemonstrateStreamingLinq();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("Improvements:");
        Console.WriteLine("- Single enumeration vs multiple re-enumerations");
        Console.WriteLine("- Materialization with ToList() eliminates redundant work");
        Console.WriteLine("- Early filtering reduces processing overhead");
        Console.WriteLine("- Take() limits unnecessary computations");
        Console.WriteLine("- Dramatically improved performance");
    }
}

================================================================================
*/
