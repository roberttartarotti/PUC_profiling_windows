/*
================================================================================
ATIVIDADE PRÁTICA 9 - BOXING/UNBOXING PERFORMANCE (C#)
================================================================================

OBJETIVO:
- Demonstrar overhead de boxing/unboxing em C#
- Usar Memory profiler para identificar alocações desnecessárias
- Otimizar usando generics e evitando boxing
- Medir impacto de boxing no GC e performance

PROBLEMA:
- Boxing cria objetos no heap para value types
- Unboxing requer cast e validação de tipo
- Memory Profiler mostrará alocações excessivas e GC pressure

SOLUÇÃO:
- Usar generics para evitar boxing
- Trabalhar com value types diretamente

================================================================================
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

class Program {
    static void DemonstrateBoxingOverhead() {
        Console.WriteLine("Starting boxing/unboxing overhead demonstration...");
        Console.WriteLine("Monitor Memory profiler - should see excessive allocations");
        
        const int ITERATIONS = 1000000;
        ArrayList boxingList = new ArrayList(); // Non-generic collection causes boxing
        
        var sw = Stopwatch.StartNew();
        long initialMemory = GC.GetTotalMemory(false);
        int initialCollections = GC.CollectionCount(0);
        
        // PERFORMANCE ISSUE: Boxing value types in non-generic collections
        for (int i = 0; i < ITERATIONS; i++) {
            boxingList.Add(i); // Boxing: int -> object allocation on heap
            
            if (i % 100000 == 0) {
                Console.WriteLine($"Boxed {i}/{ITERATIONS} integers...");
            }
        }
        
        // Now unbox them all
        long totalSum = 0;
        for (int i = 0; i < boxingList.Count; i++) {
            totalSum += (int)boxingList[i]; // Unboxing: object -> int with type check
        }
        
        sw.Stop();
        long finalMemory = GC.GetTotalMemory(false);
        int finalCollections = GC.CollectionCount(0);
        
        Console.WriteLine($"Boxing/unboxing completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Total sum: {totalSum}");
        Console.WriteLine($"Memory used: {(finalMemory - initialMemory):N0} bytes");
        Console.WriteLine($"GC collections: {finalCollections - initialCollections}");
        Console.WriteLine($"Boxed objects created: {ITERATIONS}");
    }
    
    static void Main() {
        Console.WriteLine("Starting boxing/unboxing performance demonstration...");
        Console.WriteLine("Task: Storing integers in non-generic collection");
        Console.WriteLine("Monitor Memory Usage Tool for boxing allocations");
        Console.WriteLine();
        
        DemonstrateBoxingOverhead();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check Memory profiler for:");
        Console.WriteLine("- High number of object allocations");
        Console.WriteLine("- Frequent GC collections");
        Console.WriteLine("- Memory pressure from boxing");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR A VERSÃO SEM BOXING)
================================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;

class Program {
    static void DemonstrateGenericCollection() {
        Console.WriteLine("Starting generic collection demonstration...");
        Console.WriteLine("Monitor Memory profiler - should see reduced allocations");
        
        const int ITERATIONS = 1000000;
        List<int> genericList = new List<int>(); // CORREÇÃO: Generic collection avoids boxing
        
        var sw = Stopwatch.StartNew();
        long initialMemory = GC.GetTotalMemory(false);
        int initialCollections = GC.CollectionCount(0);
        
        // CORREÇÃO: No boxing - value types stored directly
        for (int i = 0; i < ITERATIONS; i++) {
            genericList.Add(i); // No boxing: int stays as value type
            
            if (i % 100000 == 0) {
                Console.WriteLine($"Added {i}/{ITERATIONS} integers (no boxing)...");
            }
        }
        
        // Direct access without unboxing
        long totalSum = 0;
        for (int i = 0; i < genericList.Count; i++) {
            totalSum += genericList[i]; // No unboxing: direct value access
        }
        
        sw.Stop();
        long finalMemory = GC.GetTotalMemory(false);
        int finalCollections = GC.CollectionCount(0);
        
        Console.WriteLine($"Generic collection completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Total sum: {totalSum}");
        Console.WriteLine($"Memory used: {(finalMemory - initialMemory):N0} bytes");
        Console.WriteLine($"GC collections: {finalCollections - initialCollections}");
        Console.WriteLine($"No boxing/unboxing occurred");
    }
    
    static void Main() {
        Console.WriteLine("Starting optimized generic collection demonstration...");
        Console.WriteLine("Task: Storing integers in generic collection");
        Console.WriteLine("Monitor Memory Usage Tool for reduced allocations");
        Console.WriteLine();
        
        DemonstrateGenericCollection();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("Improvements:");
        Console.WriteLine("- No boxing allocations");
        Console.WriteLine("- Dramatically reduced memory usage");
        Console.WriteLine("- Fewer GC collections");
        Console.WriteLine("- Better performance due to value type semantics");
    }
}

================================================================================
*/
