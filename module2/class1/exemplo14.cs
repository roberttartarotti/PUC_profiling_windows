/*
================================================================================
ATIVIDADE PRÁTICA 14 - LARGE OBJECT HEAP (LOH) PRESSURE (C#)
================================================================================

OBJETIVO:
- Demonstrar pressure no Large Object Heap (LOH) com objetos >85KB
- Usar Memory profiler para identificar LOH allocations e GC impact
- Otimizar usando object pooling e ArrayPool<T>
- Medir impacto do LOH na performance do GC

PROBLEMA:
- Objetos >85KB vão para LOH que não é compactado frequentemente
- Muitas alocações LOH causam GC pressure e fragmentação
- Memory profiler mostrará LOH growth e Gen 2 collections

SOLUÇÃO:
- Usar ArrayPool<T> para reutilizar large arrays
- Object pooling para large objects

================================================================================
*/

using System;
using System.Buffers;
using System.Diagnostics;

class Program {
    static void DemonstrateLOHPressure() {
        Console.WriteLine("Starting Large Object Heap pressure demonstration...");
        Console.WriteLine("Monitor Memory profiler - should see LOH allocations and Gen 2 GC");
        
        const int ITERATIONS = 1000;
        const int LARGE_ARRAY_SIZE = 90000; // >85KB goes to LOH
        
        var sw = Stopwatch.StartNew();
        long initialMemory = GC.GetTotalMemory(false);
        int initialGen2Collections = GC.CollectionCount(2);
        
        for (int i = 0; i < ITERATIONS; i++) {
            // PERFORMANCE ISSUE: Creating large arrays that go to LOH
            byte[] largeArray = new byte[LARGE_ARRAY_SIZE]; // Goes to Large Object Heap
            
            // Use the array to prevent it from being optimized away
            for (int j = 0; j < 1000; j++) {
                largeArray[j] = (byte)(i % 256);
            }
            
            // Array goes out of scope - LOH pressure builds up
            
            if (i % 100 == 0) {
                long currentMemory = GC.GetTotalMemory(false);
                int currentGen2Collections = GC.CollectionCount(2);
                Console.WriteLine($"LOH allocation {i}/{ITERATIONS}:");
                Console.WriteLine($"  Memory: {currentMemory:N0} bytes");
                Console.WriteLine($"  Gen 2 collections: {currentGen2Collections}");
            }
        }
        
        sw.Stop();
        long finalMemory = GC.GetTotalMemory(false);
        int finalGen2Collections = GC.CollectionCount(2);
        
        Console.WriteLine($"LOH pressure test completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Memory increase: {(finalMemory - initialMemory):N0} bytes");
        Console.WriteLine($"Gen 2 collections triggered: {finalGen2Collections - initialGen2Collections}");
        Console.WriteLine($"Large arrays allocated: {ITERATIONS} (each {LARGE_ARRAY_SIZE:N0} bytes)");
    }
    
    static void Main() {
        Console.WriteLine("Starting Large Object Heap demonstration...");
        Console.WriteLine("Task: Creating many large arrays (>85KB)");
        Console.WriteLine("Monitor Memory Usage Tool for LOH pressure and GC impact");
        Console.WriteLine();
        
        DemonstrateLOHPressure();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check Memory profiler for:");
        Console.WriteLine("- Large Object Heap allocations");
        Console.WriteLine("- Increased Gen 2 garbage collections");
        Console.WriteLine("- Memory pressure and GC pauses");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR ARRAYPOOL)
================================================================================

using System;
using System.Buffers;
using System.Diagnostics;

class Program {
    static void DemonstrateArrayPool() {
        Console.WriteLine("Starting ArrayPool demonstration...");
        Console.WriteLine("Monitor Memory profiler - should see reduced LOH pressure");
        
        const int ITERATIONS = 1000;
        const int LARGE_ARRAY_SIZE = 90000;
        
        var sw = Stopwatch.StartNew();
        long initialMemory = GC.GetTotalMemory(false);
        int initialGen2Collections = GC.CollectionCount(2);
        
        // CORREÇÃO: Use ArrayPool to reuse large arrays
        var pool = ArrayPool<byte>.Shared;
        
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: Rent from pool instead of allocating new array
            byte[] largeArray = pool.Rent(LARGE_ARRAY_SIZE); // Reuses existing arrays
            
            try {
                // Use the array
                for (int j = 0; j < 1000; j++) {
                    largeArray[j] = (byte)(i % 256);
                }
                
                // Process data...
            }
            finally {
                // CORREÇÃO: Return to pool for reuse
                pool.Return(largeArray); // Array goes back to pool, not GC
            }
            
            if (i % 100 == 0) {
                long currentMemory = GC.GetTotalMemory(false);
                int currentGen2Collections = GC.CollectionCount(2);
                Console.WriteLine($"ArrayPool usage {i}/{ITERATIONS}:");
                Console.WriteLine($"  Memory: {currentMemory:N0} bytes");
                Console.WriteLine($"  Gen 2 collections: {currentGen2Collections}");
            }
        }
        
        sw.Stop();
        long finalMemory = GC.GetTotalMemory(false);
        int finalGen2Collections = GC.CollectionCount(2);
        
        Console.WriteLine($"ArrayPool test completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Memory increase: {(finalMemory - initialMemory):N0} bytes");
        Console.WriteLine($"Gen 2 collections triggered: {finalGen2Collections - initialGen2Collections}");
        Console.WriteLine($"Arrays reused from pool: {ITERATIONS}");
    }
    
    static void Main() {
        Console.WriteLine("Starting optimized ArrayPool demonstration...");
        Console.WriteLine("Task: Reusing large arrays with ArrayPool<T>");
        Console.WriteLine("Monitor Memory Usage Tool for reduced LOH pressure");
        Console.WriteLine();
        
        DemonstrateArrayPool();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("Improvements:");
        Console.WriteLine("- Dramatically reduced LOH allocations");
        Console.WriteLine("- Fewer Gen 2 garbage collections");
        Console.WriteLine("- Lower memory pressure");
        Console.WriteLine("- Array reuse eliminates allocation overhead");
        Console.WriteLine("- Better GC performance overall");
    }
}

================================================================================
*/
