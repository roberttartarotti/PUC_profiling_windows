/*
================================================================================
ATIVIDADE PRÁTICA 22 - PINNING E GC PRESSURE (C#)
================================================================================

OBJETIVO:
- Demonstrar problemas de performance com object pinning
- Usar Memory profiler para identificar pinned object impact
- Otimizar minimizando pinning duration e frequency
- Medir impacto de pinning na compaction do GC

PROBLEMA:
- Long-term pinning impede heap compaction
- Frequent pinning/unpinning overhead
- Memory profiler mostrará heap fragmentation

SOLUÇÃO:
- Minimize pinning duration
- Use stackalloc quando apropriado
- Batch operations to reduce pinning frequency

================================================================================
*/

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program {
    // PERFORMANCE ISSUE: Long-term pinning prevents heap compaction
    static void DemonstrateLongTermPinning() {
        Console.WriteLine("Starting long-term pinning demonstration...");
        Console.WriteLine("Monitor Memory profiler - should see heap fragmentation");
        
        const int ARRAYS = 100;
        const int ARRAY_SIZE = 50000;
        
        var pinnedHandles = new GCHandle[ARRAYS];
        var arrays = new byte[ARRAYS][];
        
        long initialMemory = GC.GetTotalMemory(false);
        int initialGen2Collections = GC.CollectionCount(2);
        
        var sw = Stopwatch.StartNew();
        
        // PERFORMANCE ISSUE: Pin many arrays for extended time
        for (int i = 0; i < ARRAYS; i++) {
            arrays[i] = new byte[ARRAY_SIZE];
            
            // Fill with data
            for (int j = 0; j < ARRAY_SIZE; j++) {
                arrays[i][j] = (byte)(j % 256);
            }
            
            // PERFORMANCE ISSUE: Pin array in memory
            pinnedHandles[i] = GCHandle.Alloc(arrays[i], GCHandleType.Pinned);
            
            if (i % 20 == 0) {
                Console.WriteLine($"Pinned {i}/{ARRAYS} arrays...");
                Console.WriteLine($"  Memory: {GC.GetTotalMemory(false):N0} bytes");
                Console.WriteLine($"  Gen 2 collections: {GC.CollectionCount(2)}");
            }
        }
        
        // Simulate work while arrays are pinned
        Console.WriteLine("Performing work while arrays are pinned...");
        
        // Force GC to show pinning impact
        for (int i = 0; i < 5; i++) {
            GC.Collect(2, GCCollectionMode.Forced, true);
            Console.WriteLine($"Forced GC #{i + 1} - Memory: {GC.GetTotalMemory(false):N0} bytes");
        }
        
        // PERFORMANCE ISSUE: Arrays were pinned during entire operation
        
        // Cleanup
        for (int i = 0; i < ARRAYS; i++) {
            if (pinnedHandles[i].IsAllocated) {
                pinnedHandles[i].Free();
            }
        }
        
        sw.Stop();
        
        long finalMemory = GC.GetTotalMemory(true); // Force cleanup
        int finalGen2Collections = GC.CollectionCount(2);
        
        Console.WriteLine($"Long-term pinning test completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Pinned arrays: {ARRAYS}");
        Console.WriteLine($"Memory after cleanup: {finalMemory:N0} bytes");
        Console.WriteLine($"Gen 2 collections: {finalGen2Collections - initialGen2Collections}");
        Console.WriteLine("Long-term pinning prevented heap compaction");
    }
    
    static void Main() {
        Console.WriteLine("Starting object pinning performance demonstration...");
        Console.WriteLine("Task: Pinning arrays in memory for extended periods");
        Console.WriteLine("Monitor Memory Usage Tool for pinning impact");
        Console.WriteLine();
        
        DemonstrateLongTermPinning();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check Memory profiler for:");
        Console.WriteLine("- Heap fragmentation due to pinned objects");
        Console.WriteLine("- Reduced GC compaction efficiency");
        Console.WriteLine("- Memory pressure from pinning");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR MINIMAL PINNING)
================================================================================

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program {
    // CORREÇÃO: Minimal pinning duration
    static void DemonstrateMinimalPinning() {
        Console.WriteLine("Starting minimal pinning demonstration...");
        Console.WriteLine("Monitor Memory profiler - should see reduced fragmentation");
        
        const int ARRAYS = 100;
        const int ARRAY_SIZE = 50000;
        const int BATCH_SIZE = 10;
        
        var arrays = new byte[ARRAYS][];
        
        // Create arrays first
        for (int i = 0; i < ARRAYS; i++) {
            arrays[i] = new byte[ARRAY_SIZE];
            for (int j = 0; j < ARRAY_SIZE; j++) {
                arrays[i][j] = (byte)(j % 256);
            }
        }
        
        long initialMemory = GC.GetTotalMemory(false);
        int initialGen2Collections = GC.CollectionCount(2);
        
        var sw = Stopwatch.StartNew();
        
        // CORREÇÃO: Process in small batches with minimal pinning duration
        for (int batch = 0; batch < ARRAYS; batch += BATCH_SIZE) {
            int batchEnd = Math.Min(batch + BATCH_SIZE, ARRAYS);
            
            // CORREÇÃO: Pin only for the duration needed
            var handles = new GCHandle[batchEnd - batch];
            
            try {
                // Pin current batch
                for (int i = 0; i < batchEnd - batch; i++) {
                    handles[i] = GCHandle.Alloc(arrays[batch + i], GCHandleType.Pinned);
                }
                
                // Process pinned arrays quickly
                for (int i = 0; i < batchEnd - batch; i++) {
                    var pinnedPtr = handles[i].AddrOfPinnedObject();
                    // Simulate native processing with pinned pointer
                    // (In real code, you'd pass this to native functions)
                }
                
            } finally {
                // CORREÇÃO: Unpin immediately after use
                for (int i = 0; i < handles.Length; i++) {
                    if (handles[i].IsAllocated) {
                        handles[i].Free();
                    }
                }
            }
            
            if (batch % 20 == 0) {
                Console.WriteLine($"Processed batch {batch}/{ARRAYS}...");
                Console.WriteLine($"  Memory: {GC.GetTotalMemory(false):N0} bytes");
            }
        }
        
        sw.Stop();
        
        // Allow GC to compact heap
        GC.Collect(2, GCCollectionMode.Optimized, true);
        
        long finalMemory = GC.GetTotalMemory(false);
        int finalGen2Collections = GC.CollectionCount(2);
        
        Console.WriteLine($"Minimal pinning completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Processed arrays: {ARRAYS}");
        Console.WriteLine($"Final memory: {finalMemory:N0} bytes");
        Console.WriteLine($"Gen 2 collections: {finalGen2Collections - initialGen2Collections}");
        Console.WriteLine("Minimal pinning allowed better heap compaction");
    }
    
    // CORREÇÃO: Use stackalloc to avoid heap allocation and pinning
    static unsafe void DemonstrateStackAllocation() {
        Console.WriteLine("Starting stackalloc demonstration...");
        
        const int ITERATIONS = 50000;
        const int BUFFER_SIZE = 1024;
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: Stack allocation - no heap allocation, no GC pressure
            Span<byte> buffer = stackalloc byte[BUFFER_SIZE];
            
            // Fill buffer
            for (int j = 0; j < BUFFER_SIZE; j++) {
                buffer[j] = (byte)(i + j);
            }
            
            // Process buffer (simulated)
            int sum = 0;
            for (int j = 0; j < BUFFER_SIZE; j++) {
                sum += buffer[j];
            }
            
            if (i % 10000 == 0) {
                Console.WriteLine($"Stackalloc iteration {i}/{ITERATIONS}, sum: {sum}");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Stackalloc completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine("No heap allocation or pinning - zero GC pressure");
    }
    
    // CORREÇÃO: Using fixed statement for temporary pinning
    static unsafe void DemonstrateFixedStatement() {
        Console.WriteLine("Starting fixed statement demonstration...");
        
        const int OPERATIONS = 10000;
        var data = new int[1000];
        
        for (int i = 0; i < data.Length; i++) {
            data[i] = i;
        }
        
        var sw = Stopwatch.StartNew();
        
        for (int op = 0; op < OPERATIONS; op++) {
            // CORREÇÃO: fixed statement provides minimal pinning scope
            fixed (int* ptr = data) {
                // Direct memory access within fixed block
                long sum = 0;
                for (int i = 0; i < data.Length; i++) {
                    sum += ptr[i]; // Direct pointer access
                }
                
                // Pinning automatically released at end of fixed block
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Fixed statement completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine("Fixed statement provided automatic pinning scope management");
    }
    
    static void Main() {
        Console.WriteLine("Starting optimized pinning demonstration...");
        Console.WriteLine("Task: Minimizing pinning duration and using alternatives");
        Console.WriteLine("Monitor Memory Usage Tool for improved GC behavior");
        Console.WriteLine();
        
        DemonstrateMinimalPinning();
        Console.WriteLine();
        DemonstrateStackAllocation();
        Console.WriteLine();
        DemonstrateFixedStatement();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("Improvements:");
        Console.WriteLine("- Minimal pinning duration reduces heap fragmentation");
        Console.WriteLine("- Batch processing reduces pinning frequency");
        Console.WriteLine("- Stackalloc eliminates heap allocation entirely");
        Console.WriteLine("- Fixed statements provide automatic scope management");
        Console.WriteLine("- Much better GC compaction and performance");
    }
}

================================================================================
*/
