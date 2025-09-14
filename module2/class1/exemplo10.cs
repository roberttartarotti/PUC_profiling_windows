/*
================================================================================
ATIVIDADE PRÁTICA 10 - PERFORMANCE DE COLEÇÕES (C#)
================================================================================

OBJETIVO:
- Demonstrar ineficiência de usar estrutura de dados inadequada
- Usar CPU profiler para identificar gargalos em operações de busca
- Otimizar escolhendo coleção adequada (List vs Dictionary/HashSet)
- Comparar complexidade O(n) vs O(1) em operações de lookup

PROBLEMA:
- Usar List<T> para muitas operações de busca é O(n)
- Contains() em List faz linear search
- CPU Profiler mostrará tempo gasto em List.Contains

SOLUÇÃO:
- Usar HashSet<T> ou Dictionary<T,V> para lookup O(1)
- Escolher coleção baseada no padrão de uso

================================================================================
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

class Program {
    static void DemonstrateInefficientLookup() {
        Console.WriteLine("Starting inefficient List lookup demonstration...");
        Console.WriteLine("Monitor CPU profiler - should see time spent in linear search");
        
        const int DATA_SIZE = 50000;
        const int LOOKUP_COUNT = 10000;
        
        // Fill List with data
        var dataList = new List<int>();
        for (int i = 0; i < DATA_SIZE; i++) {
            dataList.Add(i * 2); // Even numbers
        }
        
        var random = new Random();
        var sw = Stopwatch.StartNew();
        
        int foundCount = 0;
        for (int i = 0; i < LOOKUP_COUNT; i++) {
            int searchValue = random.Next(0, DATA_SIZE * 2);
            
            // PERFORMANCE ISSUE: Linear search O(n) in List
            if (dataList.Contains(searchValue)) {
                foundCount++;
            }
            
            if (i % 1000 == 0) {
                Console.WriteLine($"Completed {i}/{LOOKUP_COUNT} linear searches...");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"List lookup completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Found {foundCount}/{LOOKUP_COUNT} values");
        Console.WriteLine($"Average complexity per lookup: O({DATA_SIZE}) - linear search");
    }
    
    static void Main() {
        Console.WriteLine("Starting collection performance demonstration...");
        Console.WriteLine("Task: Performing many lookup operations in large dataset");
        Console.WriteLine("Monitor CPU Usage Tool for search algorithm performance");
        Console.WriteLine();
        
        DemonstrateInefficientLookup();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check CPU profiler for:");
        Console.WriteLine("- Time spent in List.Contains method");
        Console.WriteLine("- Linear search pattern in enumeration");
        Console.WriteLine("- High CPU usage due to O(n) complexity");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR A VERSÃO OTIMIZADA)
================================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;

class Program {
    static void DemonstrateEfficientLookup() {
        Console.WriteLine("Starting efficient HashSet lookup demonstration...");
        Console.WriteLine("Monitor CPU profiler - should see reduced search time");
        
        const int DATA_SIZE = 50000;
        const int LOOKUP_COUNT = 10000;
        
        // CORREÇÃO: Use HashSet for O(1) average lookup time
        var dataSet = new HashSet<int>();
        for (int i = 0; i < DATA_SIZE; i++) {
            dataSet.Add(i * 2); // Even numbers
        }
        
        var random = new Random();
        var sw = Stopwatch.StartNew();
        
        int foundCount = 0;
        for (int i = 0; i < LOOKUP_COUNT; i++) {
            int searchValue = random.Next(0, DATA_SIZE * 2);
            
            // CORREÇÃO: Hash lookup O(1) average case
            if (dataSet.Contains(searchValue)) {
                foundCount++;
            }
            
            if (i % 1000 == 0) {
                Console.WriteLine($"Completed {i}/{LOOKUP_COUNT} hash lookups...");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"HashSet lookup completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Found {foundCount}/{LOOKUP_COUNT} values");
        Console.WriteLine("Average complexity per lookup: O(1) - hash lookup");
    }
    
    static void Main() {
        Console.WriteLine("Starting optimized collection demonstration...");
        Console.WriteLine("Task: Performing lookups using hash-based collection");
        Console.WriteLine("Monitor CPU Usage Tool for improved search performance");
        Console.WriteLine();
        
        DemonstrateEfficientLookup();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("Improvements:");
        Console.WriteLine("- O(1) average lookup time vs O(n) linear search");
        Console.WriteLine("- Dramatically reduced CPU usage for searches");
        Console.WriteLine("- Constant time performance regardless of data size");
        Console.WriteLine("- Better scalability for large datasets");
    }
}

================================================================================
*/
