/*
================================================================================
ATIVIDADE PRÁTICA 24 - DELEGATE INVOCATION OVERHEAD (C#)
================================================================================

OBJETIVO:
- Demonstrar overhead de delegate calls vs direct calls
- Usar CPU profiler para identificar delegate invocation overhead
- Otimizar usando method caching e direct calls
- Medir impacto de multicast delegates

PROBLEMA:
- Delegate invocation tem overhead vs direct calls
- Multicast delegates são especialmente custosos
- CPU Profiler mostrará time spent em delegate infrastructure

SOLUÇÃO:
- Cache delegates quando possível
- Use direct calls para performance-critical paths
- Avoid multicast delegates em hot paths

================================================================================
*/

using System;
using System.Diagnostics;

class Program {
    static int SimpleCalculation(int value) {
        return value * value + 1;
    }
    
    static int AnotherCalculation(int value) {
        return value * 2 + 3;
    }
    
    static void DemonstrateDelegateOverhead() {
        Console.WriteLine("Starting delegate overhead demonstration...");
        Console.WriteLine("Monitor CPU profiler - should see delegate invocation overhead");
        
        const int ITERATIONS = 10000000;
        
        // PERFORMANCE ISSUE: Delegate calls have overhead
        Func<int, int> delegateCall = SimpleCalculation;
        
        var sw = Stopwatch.StartNew();
        
        long sum = 0;
        for (int i = 0; i < ITERATIONS; i++) {
            // PERFORMANCE ISSUE: Delegate invocation has overhead compared to direct call
            sum += delegateCall(i); // Indirect call through delegate
            
            if (i % 1000000 == 0) {
                Console.WriteLine($"Delegate calls: {i}/{ITERATIONS}");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Delegate overhead test completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Sum: {sum}");
        Console.WriteLine("Delegate calls have indirection overhead");
        
        // PERFORMANCE ISSUE: Multicast delegate overhead
        Action<int> multicastDelegate = null;
        multicastDelegate += (x) => { int temp = x * x; };
        multicastDelegate += (x) => { int temp = x + 1; };
        multicastDelegate += (x) => { int temp = x * 2; };
        
        sw.Restart();
        
        for (int i = 0; i < ITERATIONS / 10; i++) { // Fewer iterations due to higher overhead
            // PERFORMANCE ISSUE: Multicast delegate calls all methods in chain
            multicastDelegate(i); // Calls all 3 methods
            
            if (i % 100000 == 0) {
                Console.WriteLine($"Multicast delegate calls: {i}/{ITERATIONS / 10}");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Multicast delegate overhead: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine("Multicast delegates have even higher overhead");
    }
    
    static void Main() {
        Console.WriteLine("Starting delegate performance demonstration...");
        Console.WriteLine("Task: Comparing delegate vs direct method calls");
        Console.WriteLine("Monitor CPU Usage Tool for delegate invocation overhead");
        Console.WriteLine();
        
        DemonstrateDelegateOverhead();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check CPU profiler for:");
        Console.WriteLine("- Time spent in delegate invocation");
        Console.WriteLine("- Multicast delegate call overhead");
        Console.WriteLine("- Indirect call performance penalty");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR DIRECT CALLS)
================================================================================

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

class Program {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int SimpleCalculation(int value) {
        return value * value + 1;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int AnotherCalculation(int value) {
        return value * 2 + 3;
    }
    
    static void DemonstrateDirectCallOptimization() {
        Console.WriteLine("Starting direct call optimization demonstration...");
        Console.WriteLine("Monitor CPU profiler - should see reduced call overhead");
        
        const int ITERATIONS = 10000000;
        
        var sw = Stopwatch.StartNew();
        
        long sum = 0;
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: Direct method call - can be inlined by JIT
            sum += SimpleCalculation(i); // Direct call, no indirection
            
            if (i % 1000000 == 0) {
                Console.WriteLine($"Direct calls: {i}/{ITERATIONS}");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Direct call optimization completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Sum: {sum}");
        Console.WriteLine("Direct calls can be inlined for maximum performance");
    }
    
    // CORREÇÃO: Cached delegate to reduce repeated allocation
    private static readonly Func<int, int> CachedDelegate = SimpleCalculation;
    
    static void DemonstrateCachedDelegates() {
        Console.WriteLine("Starting cached delegate demonstration...");
        
        const int ITERATIONS = 10000000;
        
        var sw = Stopwatch.StartNew();
        
        long sum = 0;
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: Use cached delegate - no repeated allocation
            sum += CachedDelegate(i);
        }
        
        sw.Stop();
        
        Console.WriteLine($"Cached delegate completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Sum: {sum}");
        Console.WriteLine("Cached delegate eliminates repeated allocation overhead");
    }
    
    // CORREÇÃO: Interface-based approach for polymorphism without delegate overhead
    interface ICalculator {
        int Calculate(int value);
    }
    
    class SimpleCalculator : ICalculator {
        public int Calculate(int value) => value * value + 1;
    }
    
    class AnotherCalculator : ICalculator {
        public int Calculate(int value) => value * 2 + 3;
    }
    
    static void DemonstrateInterfaceApproach() {
        Console.WriteLine("Starting interface-based approach demonstration...");
        
        const int ITERATIONS = 10000000;
        
        ICalculator calculator = new SimpleCalculator();
        
        var sw = Stopwatch.StartNew();
        
        long sum = 0;
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: Interface call - JIT can optimize virtual calls
            sum += calculator.Calculate(i);
        }
        
        sw.Stop();
        
        Console.WriteLine($"Interface approach completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Sum: {sum}");
        Console.WriteLine("Interface calls can be optimized by JIT compiler");
    }
    
    // CORREÇÃO: Function pointer approach for maximum performance
    static unsafe void DemonstrateFunctionPointers() {
        Console.WriteLine("Starting function pointer demonstration...");
        
        const int ITERATIONS = 10000000;
        
        // Function pointer - C# 9 feature
        delegate*<int, int> funcPtr = &SimpleCalculation;
        
        var sw = Stopwatch.StartNew();
        
        long sum = 0;
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: Function pointer call - minimal overhead
            sum += funcPtr(i);
        }
        
        sw.Stop();
        
        Console.WriteLine($"Function pointer completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Sum: {sum}");
        Console.WriteLine("Function pointers provide near-direct-call performance");
    }
    
    // CORREÇÃO: Avoid multicast delegates in hot paths
    static void DemonstrateDirectMultipleOperations() {
        Console.WriteLine("Starting direct multiple operations demonstration...");
        
        const int ITERATIONS = 1000000;
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: Call methods directly instead of multicast delegate
            int temp1 = i * i;     // Direct operation 1
            int temp2 = i + 1;     // Direct operation 2  
            int temp3 = i * 2;     // Direct operation 3
            
            if (i % 100000 == 0) {
                Console.WriteLine($"Direct multiple operations: {i}/{ITERATIONS}");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Direct multiple operations completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine("Direct operations much faster than multicast delegates");
    }
    
    // CORREÇÃO: Generic approach for type-safe performance
    static void DemonstrateGenericApproach<TCalculator>() 
        where TCalculator : ICalculator, new() {
        
        Console.WriteLine("Starting generic approach demonstration...");
        
        const int ITERATIONS = 10000000;
        
        var calculator = new TCalculator();
        
        var sw = Stopwatch.StartNew();
        
        long sum = 0;
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: Generic constraint allows JIT to devirtualize calls
            sum += calculator.Calculate(i);
        }
        
        sw.Stop();
        
        Console.WriteLine($"Generic approach completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Sum: {sum}");
        Console.WriteLine("Generic constraints enable JIT optimization");
    }
    
    static void Main() {
        Console.WriteLine("Starting optimized delegate alternatives demonstration...");
        Console.WriteLine("Task: Comparing different approaches to method calls");
        Console.WriteLine("Monitor CPU Usage Tool for performance improvements");
        Console.WriteLine();
        
        DemonstrateDirectCallOptimization();
        Console.WriteLine();
        DemonstrateCachedDelegates();
        Console.WriteLine();
        DemonstrateInterfaceApproach();
        Console.WriteLine();
        DemonstrateFunctionPointers();
        Console.WriteLine();
        DemonstrateDirectMultipleOperations();
        Console.WriteLine();
        DemonstrateGenericApproach<SimpleCalculator>();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("Performance ranking (fastest to slowest):");
        Console.WriteLine("1. Function pointers - near-direct call performance");
        Console.WriteLine("2. Direct calls with inlining - excellent performance");
        Console.WriteLine("3. Generic constrained calls - JIT optimized");
        Console.WriteLine("4. Interface calls - good performance");
        Console.WriteLine("5. Cached delegates - reduced allocation overhead");
        Console.WriteLine("6. Fresh delegates - allocation + call overhead");
        Console.WriteLine("7. Multicast delegates - highest overhead");
    }
}

================================================================================
*/
