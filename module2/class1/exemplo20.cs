/*
================================================================================
ATIVIDADE PRÁTICA 20 - UNSAFE CODE E PERFORMANCE CRÍTICA (C#)
================================================================================

OBJETIVO:
- Demonstrar quando unsafe code pode melhorar performance crítica
- Usar Memory/CPU profiler para comparar managed vs unsafe operations
- Otimizar usando unsafe pointers e fixed arrays
- Medir diferença entre bounds checking vs direct memory access

PROBLEMA:
- Bounds checking em arrays tem overhead
- Managed memory access é mais lento que direct pointers
- Memory profiler mostrará overhead de managed operations

SOLUÇÃO:
- Unsafe code para performance-critical sections
- Fixed buffers para direct memory access
- Pointer arithmetic para maximum speed

================================================================================
*/

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program {
    static void DemonstrateSafeManagedPerformance() {
        Console.WriteLine("Starting safe managed code performance demonstration...");
        Console.WriteLine("Monitor Memory/CPU profiler - should see bounds checking overhead");
        
        const int ARRAY_SIZE = 10000000; // 10 million elements
        const int ITERATIONS = 10;
        
        int[] sourceArray = new int[ARRAY_SIZE];
        int[] destArray = new int[ARRAY_SIZE];
        
        // Fill source array
        var random = new Random(42);
        for (int i = 0; i < ARRAY_SIZE; i++) {
            sourceArray[i] = random.Next(1000);
        }
        
        var sw = Stopwatch.StartNew();
        
        for (int iter = 0; iter < ITERATIONS; iter++) {
            // PERFORMANCE ISSUE: Bounds checking on every array access
            for (int i = 0; i < ARRAY_SIZE; i++) {
                // Each array access has bounds checking overhead
                int value = sourceArray[i];     // Bounds check #1
                value = value * 2 + 1;          // Arithmetic operation
                destArray[i] = value;           // Bounds check #2
            }
            
            if (iter % 2 == 0) {
                Console.WriteLine($"Safe iteration {iter + 1}/{ITERATIONS} completed");
            }
        }
        
        sw.Stop();
        
        long sum = 0;
        for (int i = 0; i < Math.Min(1000, ARRAY_SIZE); i++) {
            sum += destArray[i];
        }
        
        Console.WriteLine($"Safe managed code completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Array size: {ARRAY_SIZE:N0} elements");
        Console.WriteLine($"Total operations: {(long)ARRAY_SIZE * ITERATIONS * 2:N0} (with bounds checking)");
        Console.WriteLine($"Sample sum: {sum}");
    }
    
    static void Main() {
        Console.WriteLine("Starting managed vs unsafe performance demonstration...");
        Console.WriteLine("Task: Array processing with bounds checking overhead");
        Console.WriteLine("Monitor profilers for managed code overhead");
        Console.WriteLine();
        
        DemonstrateSafeManagedPerformance();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check profilers for:");
        Console.WriteLine("- Bounds checking overhead on array access");
        Console.WriteLine("- Managed memory access patterns");
        Console.WriteLine("- JIT optimization limitations with bounds checks");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR UNSAFE CODE)
================================================================================

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

unsafe class Program {
    // CORREÇÃO: Unsafe method for maximum performance
    static void DemonstrateUnsafePerformance() {
        Console.WriteLine("Starting unsafe code performance demonstration...");
        Console.WriteLine("Monitor Memory/CPU profiler - should see reduced overhead");
        
        const int ARRAY_SIZE = 10000000;
        const int ITERATIONS = 10;
        
        int[] sourceArray = new int[ARRAY_SIZE];
        int[] destArray = new int[ARRAY_SIZE];
        
        // Fill source array
        var random = new Random(42);
        for (int i = 0; i < ARRAY_SIZE; i++) {
            sourceArray[i] = random.Next(1000);
        }
        
        var sw = Stopwatch.StartNew();
        
        // CORREÇÃO: Pin arrays in memory for direct pointer access
        fixed (int* srcPtr = sourceArray)
        fixed (int* destPtr = destArray) {
            for (int iter = 0; iter < ITERATIONS; iter++) {
                // CORREÇÃO: Direct pointer arithmetic - no bounds checking
                int* src = srcPtr;
                int* dest = destPtr;
                
                for (int i = 0; i < ARRAY_SIZE; i++) {
                    int value = *src++;         // Direct memory access - no bounds check
                    value = value * 2 + 1;      // Same arithmetic
                    *dest++ = value;            // Direct memory write - no bounds check
                }
                
                if (iter % 2 == 0) {
                    Console.WriteLine($"Unsafe iteration {iter + 1}/{ITERATIONS} completed");
                }
            }
        }
        
        sw.Stop();
        
        long sum = 0;
        for (int i = 0; i < Math.Min(1000, ARRAY_SIZE); i++) {
            sum += destArray[i];
        }
        
        Console.WriteLine($"Unsafe code completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Array size: {ARRAY_SIZE:N0} elements");
        Console.WriteLine($"Total operations: {(long)ARRAY_SIZE * ITERATIONS * 2:N0} (no bounds checking)");
        Console.WriteLine($"Sample sum: {sum}");
    }
    
    // CORREÇÃO: Unsafe struct for high-performance data processing
    struct UnsafeImageData {
        public fixed byte pixels[1920 * 1080 * 3]; // Fixed buffer - stack allocated
    }
    
    static void DemonstrateUnsafeImageProcessing() {
        Console.WriteLine("Starting unsafe image processing demonstration...");
        
        const int WIDTH = 1920;
        const int HEIGHT = 1080;
        const int CHANNELS = 3; // RGB
        
        var sw = Stopwatch.StartNew();
        
        // CORREÇÃO: Stack-allocated fixed buffer for maximum performance
        UnsafeImageData imageData = new UnsafeImageData();
        
        // Initialize with test pattern
        fixed (byte* pixelPtr = imageData.pixels) {
            byte* ptr = pixelPtr;
            for (int i = 0; i < WIDTH * HEIGHT * CHANNELS; i++) {
                *ptr++ = (byte)(i % 256);
            }
        }
        
        // CORREÇÃO: High-performance image processing with direct memory access
        fixed (byte* pixelPtr = imageData.pixels) {
            byte* ptr = pixelPtr;
            int totalPixels = WIDTH * HEIGHT;
            
            // Convert RGB to grayscale using pointer arithmetic
            for (int i = 0; i < totalPixels; i++) {
                byte r = *ptr;
                byte g = *(ptr + 1);
                byte b = *(ptr + 2);
                
                // Grayscale conversion
                byte gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
                
                // Write back as grayscale
                *ptr++ = gray;      // R
                *ptr++ = gray;      // G  
                *ptr++ = gray;      // B
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Unsafe image processing completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Image size: {WIDTH}x{HEIGHT} pixels");
        Console.WriteLine($"Direct memory operations: {WIDTH * HEIGHT * CHANNELS:N0}");
    }
    
    static void DemonstrateSpanPerformance() {
        Console.WriteLine("Starting Span<T> performance demonstration...");
        Console.WriteLine("Span<T> provides safe high-performance alternative to unsafe code");
        
        const int ARRAY_SIZE = 10000000;
        const int ITERATIONS = 10;
        
        int[] sourceArray = new int[ARRAY_SIZE];
        int[] destArray = new int[ARRAY_SIZE];
        
        var random = new Random(42);
        for (int i = 0; i < ARRAY_SIZE; i++) {
            sourceArray[i] = random.Next(1000);
        }
        
        var sw = Stopwatch.StartNew();
        
        // CORREÇÃO: Span<T> provides near-unsafe performance with safety
        Span<int> srcSpan = sourceArray.AsSpan();
        Span<int> destSpan = destArray.AsSpan();
        
        for (int iter = 0; iter < ITERATIONS; iter++) {
            // CORREÇÃO: Span access is nearly as fast as unsafe, but with bounds checking in debug
            for (int i = 0; i < ARRAY_SIZE; i++) {
                int value = srcSpan[i];      // Highly optimized access
                value = value * 2 + 1;
                destSpan[i] = value;         // Highly optimized write
            }
        }
        
        sw.Stop();
        
        long sum = 0;
        for (int i = 0; i < Math.Min(1000, ARRAY_SIZE); i++) {
            sum += destArray[i];
        }
        
        Console.WriteLine($"Span<T> processing completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Span<T> provides safe high-performance alternative");
        Console.WriteLine($"Sample sum: {sum}");
    }
    
    static void Main() {
        Console.WriteLine("Starting optimized unsafe/high-performance demonstration...");
        Console.WriteLine("Task: Comparing managed, unsafe, and Span<T> performance");
        Console.WriteLine("Monitor profilers for performance improvements");
        Console.WriteLine();
        
        DemonstrateUnsafePerformance();
        Console.WriteLine();
        DemonstrateUnsafeImageProcessing();
        Console.WriteLine();
        DemonstrateSpanPerformance();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("Performance ranking (fastest to slowest):");
        Console.WriteLine("1. Unsafe code with fixed pointers - maximum performance");
        Console.WriteLine("2. Span<T> - near-unsafe performance with safety");
        Console.WriteLine("3. Regular managed arrays - bounds checking overhead");
        Console.WriteLine();
        Console.WriteLine("Improvements:");
        Console.WriteLine("- Unsafe code eliminates bounds checking");
        Console.WriteLine("- Fixed buffers avoid managed heap allocation");
        Console.WriteLine("- Direct pointer arithmetic is fastest possible");
        Console.WriteLine("- Span<T> provides good compromise between safety and speed");
    }
}

================================================================================
*/
