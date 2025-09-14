/*
================================================================================
ATIVIDADE PRÁTICA 26 - REFERENCE TYPE OVERHEAD (C#)
================================================================================

OBJETIVO:
- Demonstrar overhead de reference types vs value types
- Usar Memory/CPU profiler para identificar allocation overhead
- Otimizar usando value types e structs
- Medir diferença entre class vs struct performance

PROBLEMA:
- Reference types require heap allocation
- GC pressure from many small objects
- Memory profiler mostrará excessive allocations

SOLUÇÃO:
- Use value types para small data
- Struct instead of class quando apropriado
- Reduce boxing/unboxing overhead

================================================================================
*/

using System;
using System.Diagnostics;

// PERFORMANCE ISSUE: Reference type for small data
class PointClass {
    public double X { get; set; }
    public double Y { get; set; }
    
    public PointClass(double x, double y) {
        X = x;
        Y = y;
    }
    
    public double DistanceFromOrigin() {
        return Math.Sqrt(X * X + Y * Y);
    }
}

class Program {
    static void DemonstrateReferenceTypeOverhead() {
        Console.WriteLine("Starting reference type overhead demonstration...");
        Console.WriteLine("Monitor Memory profiler - should see excessive heap allocations");
        
        const int ITERATIONS = 1000000;
        
        long initialMemory = GC.GetTotalMemory(false);
        int initialCollections = GC.CollectionCount(0);
        
        var sw = Stopwatch.StartNew();
        
        double totalDistance = 0;
        
        for (int i = 0; i < ITERATIONS; i++) {
            // PERFORMANCE ISSUE: Heap allocation for each point
            var point = new PointClass(i * 0.1, i * 0.2); // Heap allocation
            totalDistance += point.DistanceFromOrigin();
            
            if (i % 100000 == 0) {
                Console.WriteLine($"Reference type: {i}/{ITERATIONS}, Memory: {GC.GetTotalMemory(false):N0} bytes");
            }
        }
        
        sw.Stop();
        
        long finalMemory = GC.GetTotalMemory(false);
        int finalCollections = GC.CollectionCount(0);
        
        Console.WriteLine($"Reference type overhead completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Total distance: {totalDistance:F2}");
        Console.WriteLine($"Memory growth: {(finalMemory - initialMemory):N0} bytes");
        Console.WriteLine($"GC collections: {finalCollections - initialCollections}");
        Console.WriteLine($"Objects allocated: {ITERATIONS}");
    }
    
    static void Main() {
        Console.WriteLine("Starting reference vs value type demonstration...");
        Console.WriteLine("Task: Creating many small objects using reference types");
        Console.WriteLine("Monitor Memory Usage Tool for allocation overhead");
        Console.WriteLine();
        
        DemonstrateReferenceTypeOverhead();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check Memory profiler for:");
        Console.WriteLine("- High number of heap allocations");
        Console.WriteLine("- GC pressure from many small objects");
        Console.WriteLine("- Memory overhead from reference types");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR VALUE TYPES)
================================================================================

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

// CORREÇÃO: Value type for small data
public readonly struct PointStruct {
    public double X { get; }
    public double Y { get; }
    
    public PointStruct(double x, double y) {
        X = x;
        Y = y;
    }
    
    public double DistanceFromOrigin() {
        return Math.Sqrt(X * X + Y * Y);
    }
}

// CORREÇÃO: ref struct for even better performance (stack-only)
public ref struct PointRefStruct {
    public double X;
    public double Y;
    
    public PointRefStruct(double x, double y) {
        X = x;
        Y = y;
    }
    
    public readonly double DistanceFromOrigin() {
        return Math.Sqrt(X * X + Y * Y);
    }
}

class Program {
    static void DemonstrateValueTypeOptimization() {
        Console.WriteLine("Starting value type optimization demonstration...");
        Console.WriteLine("Monitor Memory profiler - should see reduced allocations");
        
        const int ITERATIONS = 1000000;
        
        long initialMemory = GC.GetTotalMemory(false);
        int initialCollections = GC.CollectionCount(0);
        
        var sw = Stopwatch.StartNew();
        
        double totalDistance = 0;
        
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: Stack allocation - no heap allocation
            var point = new PointStruct(i * 0.1, i * 0.2); // Stack allocation
            totalDistance += point.DistanceFromOrigin();
            
            if (i % 100000 == 0) {
                Console.WriteLine($"Value type: {i}/{ITERATIONS}, Memory: {GC.GetTotalMemory(false):N0} bytes");
            }
        }
        
        sw.Stop();
        
        long finalMemory = GC.GetTotalMemory(false);
        int finalCollections = GC.CollectionCount(0);
        
        Console.WriteLine($"Value type optimization completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Total distance: {totalDistance:F2}");
        Console.WriteLine($"Memory growth: {(finalMemory - initialMemory):N0} bytes");
        Console.WriteLine($"GC collections: {finalCollections - initialCollections}");
        Console.WriteLine("No heap allocations - all stack-based");
    }
    
    static void DemonstrateRefStructPerformance() {
        Console.WriteLine("Starting ref struct performance demonstration...");
        
        const int ITERATIONS = 1000000;
        
        var sw = Stopwatch.StartNew();
        
        double totalDistance = 0;
        
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: ref struct - guaranteed stack allocation
            var point = new PointRefStruct(i * 0.1, i * 0.2);
            totalDistance += point.DistanceFromOrigin();
        }
        
        sw.Stop();
        
        Console.WriteLine($"Ref struct performance completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Total distance: {totalDistance:F2}");
        Console.WriteLine("Ref struct guarantees stack allocation - zero GC pressure");
    }
    
    static void DemonstrateSpanPerformance() {
        Console.WriteLine("Starting Span<T> performance demonstration...");
        
        const int ARRAY_SIZE = 1000000;
        
        // CORREÇÃO: Use Span<T> for efficient array slicing without allocation
        var data = new double[ARRAY_SIZE];
        for (int i = 0; i < ARRAY_SIZE; i++) {
            data[i] = i * 0.001;
        }
        
        var sw = Stopwatch.StartNew();
        
        double sum = 0;
        
        // CORREÇÃO: Span operations don't allocate
        Span<double> span = data.AsSpan();
        
        for (int i = 0; i < ARRAY_SIZE - 1; i++) {
            // Efficient slice operation - no allocation
            Span<double> slice = span.Slice(i, 2);
            sum += slice[0] * slice[1];
        }
        
        sw.Stop();
        
        Console.WriteLine($"Span<T> performance completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Sum: {sum:F2}");
        Console.WriteLine("Span<T> operations created no heap allocations");
    }
    
    // CORREÇÃO: Demonstrate avoiding boxing
    static void DemonstrateBoxingAvoidance() {
        Console.WriteLine("Starting boxing avoidance demonstration...");
        
        const int ITERATIONS = 1000000;
        
        var sw = Stopwatch.StartNew();
        
        // CORREÇÃO: Generic collection avoids boxing
        var points = new PointStruct[ITERATIONS];
        
        for (int i = 0; i < ITERATIONS; i++) {
            points[i] = new PointStruct(i * 0.1, i * 0.2);
        }
        
        double totalDistance = 0;
        foreach (var point in points) {
            totalDistance += point.DistanceFromOrigin(); // No boxing
        }
        
        sw.Stop();
        
        Console.WriteLine($"Boxing avoidance completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Total distance: {totalDistance:F2}");
        Console.WriteLine("Generic collections avoid boxing value types");
    }
    
    // CORREÇÃO: Demonstrate stackalloc for maximum performance
    static unsafe void DemonstrateStackAlloc() {
        Console.WriteLine("Starting stackalloc demonstration...");
        
        const int BUFFER_SIZE = 10000;
        
        var sw = Stopwatch.StartNew();
        
        // CORREÇÃO: Stack allocation for temporary buffers
        Span<double> buffer = stackalloc double[BUFFER_SIZE];
        
        // Fill buffer with calculations
        for (int i = 0; i < BUFFER_SIZE; i++) {
            buffer[i] = Math.Sin(i) * Math.Cos(i);
        }
        
        // Process buffer
        double sum = 0;
        foreach (var value in buffer) {
            sum += value;
        }
        
        sw.Stop();
        
        Console.WriteLine($"Stackalloc completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Sum: {sum:F6}");
        Console.WriteLine("Stackalloc provided zero-allocation temporary storage");
    }
    
    static void Main() {
        Console.WriteLine("Starting optimized value type demonstration...");
        Console.WriteLine("Task: Using value types to eliminate heap allocations");
        Console.WriteLine("Monitor Memory Usage Tool for reduced allocation pressure");
        Console.WriteLine();
        
        DemonstrateValueTypeOptimization();
        Console.WriteLine();
        DemonstrateRefStructPerformance();
        Console.WriteLine();
        DemonstrateSpanPerformance();
        Console.WriteLine();
        DemonstrateBoxingAvoidance();
        Console.WriteLine();
        DemonstrateStackAlloc();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("Improvements:");
        Console.WriteLine("- Value types eliminate heap allocations");
        Console.WriteLine("- Ref structs guarantee stack allocation");
        Console.WriteLine("- Span<T> provides zero-allocation array operations");
        Console.WriteLine("- Generic collections avoid boxing");
        Console.WriteLine("- Stackalloc eliminates temporary allocations");
        Console.WriteLine("- Dramatically reduced GC pressure and memory usage");
    }
}

================================================================================
*/
