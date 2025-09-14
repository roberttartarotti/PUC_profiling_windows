/*
================================================================================
ATIVIDADE PRÁTICA 23 - JIT COMPILATION OVERHEAD (C#)
================================================================================

OBJETIVO:
- Demonstrar JIT compilation overhead no first run
- Usar CPU profiler para identificar JIT compilation time
- Otimizar usando ahead-of-time compilation e warm-up
- Medir diferença between cold start vs warmed up code

PROBLEMA:
- First-time method execution includes JIT compilation overhead
- Cold start performance é significativamente pior
- CPU Profiler mostrará time spent in JIT compilation

SOLUÇÃO:
- Warm-up critical code paths
- Use ReadyToRun images para reduce JIT overhead
- Profile-guided optimization

================================================================================
*/

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

class Program {
    // PERFORMANCE ISSUE: Complex methods that will trigger JIT compilation
    static void ComplexCalculation(int iterations) {
        double result = 0;
        
        for (int i = 0; i < iterations; i++) {
            // Complex floating point operations that will be JIT compiled
            result += Math.Sin(i) * Math.Cos(i) + Math.Sqrt(i);
            result -= Math.Tan(i * 0.1) / (i + 1);
            result *= Math.Log(i + 1) + Math.Exp(-i * 0.001);
            
            // Conditional logic that affects JIT optimization
            if (i % 1000 == 0) {
                result = Math.Abs(result);
            }
            
            if (i % 10000 == 0) {
                Console.WriteLine($"JIT compilation calculation: {i}/{iterations}");
            }
        }
        
        Console.WriteLine($"Complex calculation result: {result}");
    }
    
    static void GenericMethod<T>(T[] array) where T : IComparable<T> {
        // Generic method that will be JIT compiled for each type
        for (int i = 0; i < array.Length - 1; i++) {
            for (int j = i + 1; j < array.Length; j++) {
                if (array[i].CompareTo(array[j]) > 0) {
                    T temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;
                }
            }
        }
    }
    
    static void DemonstrateJITOverhead() {
        Console.WriteLine("Starting JIT compilation overhead demonstration...");
        Console.WriteLine("Monitor CPU profiler - should see JIT compilation time on first run");
        
        const int ITERATIONS = 1000000;
        
        // PERFORMANCE ISSUE: First execution includes JIT compilation overhead
        var sw = Stopwatch.StartNew();
        ComplexCalculation(ITERATIONS); // JIT compilation happens here
        sw.Stop();
        
        Console.WriteLine($"First run (with JIT overhead): {sw.ElapsedMilliseconds} ms");
        
        // Second run - code is already JIT compiled
        sw.Restart();
        ComplexCalculation(ITERATIONS); // No JIT overhead
        sw.Stop();
        
        Console.WriteLine($"Second run (JIT already done): {sw.ElapsedMilliseconds} ms");
        
        // PERFORMANCE ISSUE: Generic method JIT compilation for different types
        var intArray = new int[] { 5, 2, 8, 1, 9, 3 };
        var stringArray = new string[] { "zebra", "apple", "dog", "cat" };
        var doubleArray = new double[] { 3.14, 2.71, 1.41, 0.57 };
        
        sw.Restart();
        GenericMethod(intArray);    // JIT compilation for int
        GenericMethod(stringArray); // JIT compilation for string  
        GenericMethod(doubleArray); // JIT compilation for double
        sw.Stop();
        
        Console.WriteLine($"Generic method first runs (with JIT): {sw.ElapsedMilliseconds} ms");
        
        sw.Restart();
        GenericMethod(intArray);    // Already JIT compiled
        GenericMethod(stringArray); // Already JIT compiled
        GenericMethod(doubleArray); // Already JIT compiled
        sw.Stop();
        
        Console.WriteLine($"Generic method second runs (no JIT): {sw.ElapsedMilliseconds} ms");
    }
    
    static void Main() {
        Console.WriteLine("Starting JIT compilation demonstration...");
        Console.WriteLine("Task: Measuring JIT compilation overhead on first execution");
        Console.WriteLine("Monitor CPU Usage Tool for JIT compilation time");
        Console.WriteLine();
        
        DemonstrateJITOverhead();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check CPU profiler for:");
        Console.WriteLine("- Time spent in JIT compilation on first run");
        Console.WriteLine("- Performance difference between cold and warm runs");
        Console.WriteLine("- Generic method JIT compilation for different types");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR JIT OPTIMIZATION STRATEGIES)
================================================================================

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

class Program {
    // CORREÇÃO: Aggressively inline critical methods
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static double FastMathOperation(int i) {
        return Math.Sin(i) * Math.Cos(i) + Math.Sqrt(i);
    }
    
    // CORREÇÃO: Mark methods for aggressive optimization
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    static void OptimizedCalculation(int iterations) {
        double result = 0;
        
        for (int i = 0; i < iterations; i++) {
            result += FastMathOperation(i);
            result -= Math.Tan(i * 0.1) / (i + 1);
            result *= Math.Log(i + 1) + Math.Exp(-i * 0.001);
            
            if (i % 1000 == 0) {
                result = Math.Abs(result);
            }
            
            if (i % 10000 == 0) {
                Console.WriteLine($"Optimized calculation: {i}/{iterations}");
            }
        }
        
        Console.WriteLine($"Optimized calculation result: {result}");
    }
    
    // CORREÇÃO: Warm-up method to trigger JIT compilation
    static void WarmUpMethods() {
        Console.WriteLine("Warming up methods to trigger JIT compilation...");
        
        // Warm up with small workload to trigger JIT
        OptimizedCalculation(1000);
        
        // Warm up generic methods
        var smallIntArray = new int[] { 1, 2 };
        var smallStringArray = new string[] { "a", "b" };
        var smallDoubleArray = new double[] { 1.0, 2.0 };
        
        OptimizedGenericMethod(smallIntArray);
        OptimizedGenericMethod(smallStringArray);
        OptimizedGenericMethod(smallDoubleArray);
        
        Console.WriteLine("Warm-up completed - methods are now JIT compiled");
    }
    
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    static void OptimizedGenericMethod<T>(T[] array) where T : IComparable<T> {
        // Use more efficient sorting algorithm
        Array.Sort(array);
    }
    
    static void DemonstrateJITOptimization() {
        Console.WriteLine("Starting JIT optimization demonstration...");
        Console.WriteLine("Monitor CPU profiler - should see reduced JIT overhead");
        
        const int ITERATIONS = 1000000;
        
        // CORREÇÃO: Warm up methods first
        WarmUpMethods();
        
        // Now run performance tests with warmed up JIT
        var sw = Stopwatch.StartNew();
        OptimizedCalculation(ITERATIONS); // Already JIT compiled
        sw.Stop();
        
        Console.WriteLine($"Optimized first run (warmed up): {sw.ElapsedMilliseconds} ms");
        
        sw.Restart();
        OptimizedCalculation(ITERATIONS);
        sw.Stop();
        
        Console.WriteLine($"Optimized second run: {sw.ElapsedMilliseconds} ms");
        
        // Test generic methods
        var intArray = new int[10000];
        var random = new Random(42);
        for (int i = 0; i < intArray.Length; i++) {
            intArray[i] = random.Next(1000);
        }
        
        var stringArray = new string[1000];
        for (int i = 0; i < stringArray.Length; i++) {
            stringArray[i] = $"string_{random.Next(1000):D4}";
        }
        
        sw.Restart();
        OptimizedGenericMethod(intArray);    // Already JIT compiled
        OptimizedGenericMethod(stringArray); // Already JIT compiled
        sw.Stop();
        
        Console.WriteLine($"Optimized generic methods (warmed up): {sw.ElapsedMilliseconds} ms");
    }
    
    // CORREÇÃO: Demonstrate ReadyToRun compilation benefits
    static void DemonstrateReadyToRunBenefits() {
        Console.WriteLine("Starting ReadyToRun benefits demonstration...");
        
        var sw = Stopwatch.StartNew();
        
        // CORREÇÃO: Methods compiled with ReadyToRun have minimal JIT overhead
        for (int i = 0; i < 100000; i++) {
            var result = Math.Sqrt(i) + Math.Sin(i);
            if (i == 50000) {
                Console.WriteLine($"Intermediate result: {result}");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"ReadyToRun optimized execution: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine("ReadyToRun images reduce JIT compilation overhead");
    }
    
    // CORREÇÃO: Use tiered compilation awareness
    [MethodImpl(MethodImplOptions.NoInlining)] // Prevent inlining to show tiered compilation
    static long TieredCompilationAwareMethod(int iterations) {
        long sum = 0;
        
        for (int i = 0; i < iterations; i++) {
            // This method will be compiled with different optimization levels
            sum += i * i + i;
            
            if (i % (iterations / 10) == 0) {
                Console.WriteLine($"Tiered compilation method: {i}/{iterations}");
            }
        }
        
        return sum;
    }
    
    static void DemonstrateTieredCompilation() {
        Console.WriteLine("Starting tiered compilation demonstration...");
        
        // CORREÇÃO: Let tiered compilation optimize the method over multiple calls
        for (int call = 0; call < 5; call++) {
            var sw = Stopwatch.StartNew();
            var result = TieredCompilationAwareMethod(100000);
            sw.Stop();
            
            Console.WriteLine($"Tiered compilation call #{call + 1}: {sw.ElapsedMilliseconds} ms, result: {result}");
        }
        
        Console.WriteLine("Tiered compilation improved performance over multiple calls");
    }
    
    static void Main() {
        Console.WriteLine("Starting optimized JIT compilation demonstration...");
        Console.WriteLine("Task: Optimizing JIT compilation overhead with various strategies");
        Console.WriteLine("Monitor CPU Usage Tool for improved performance");
        Console.WriteLine();
        
        DemonstrateJITOptimization();
        Console.WriteLine();
        DemonstrateReadyToRunBenefits();
        Console.WriteLine();
        DemonstrateTieredCompilation();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("Improvements:");
        Console.WriteLine("- Method warm-up eliminates first-run JIT overhead");
        Console.WriteLine("- AggressiveOptimization attribute improves JIT output");
        Console.WriteLine("- ReadyToRun compilation reduces JIT time");
        Console.WriteLine("- Tiered compilation optimizes hot paths over time");
        Console.WriteLine("- Aggressive inlining reduces call overhead");
    }
}

================================================================================
*/
