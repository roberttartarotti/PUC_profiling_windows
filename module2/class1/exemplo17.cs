/*
================================================================================
ATIVIDADE PRÁTICA 17 - REFLECTION PERFORMANCE OVERHEAD (C#)
================================================================================

OBJETIVO:
- Demonstrar overhead de reflection em operações repetitivas
- Usar CPU profiler para identificar tempo gasto em reflection calls
- Otimizar usando caching, delegates, ou compiled expressions
- Medir diferença entre reflection vs direct calls

PROBLEMA:
- Reflection é muito mais lenta que direct method calls
- Repeated Type.GetMethod() e MethodInfo.Invoke() são custosos
- CPU Profiler mostrará tempo gasto em reflection infrastructure

SOLUÇÃO:
- Cache MethodInfo objects
- Use delegates ou compiled expressions para melhor performance

================================================================================
*/

using System;
using System.Reflection;
using System.Diagnostics;

class DataProcessor {
    public int ProcessValue(int value) {
        return value * value + 1;
    }
    
    public string ProcessString(string input) {
        return input?.ToUpper() + "_PROCESSED";
    }
}

class Program {
    static void DemonstrateReflectionOverhead() {
        Console.WriteLine("Starting reflection overhead demonstration...");
        Console.WriteLine("Monitor CPU profiler - should see time spent in reflection calls");
        
        const int ITERATIONS = 100000;
        var processor = new DataProcessor();
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < ITERATIONS; i++) {
            // PERFORMANCE ISSUE: Repeated reflection calls are very expensive
            Type type = processor.GetType();                                    // Expensive
            MethodInfo method = type.GetMethod("ProcessValue");                 // Very expensive
            object result = method.Invoke(processor, new object[] { i });      // Extremely expensive
            
            // More reflection overhead
            MethodInfo stringMethod = type.GetMethod("ProcessString");          // Expensive again
            object stringResult = stringMethod.Invoke(processor, new object[] { $"Item_{i}" }); // Expensive
            
            if (i % 10000 == 0) {
                Console.WriteLine($"Reflection call {i}/{ITERATIONS}: {result}, {stringResult}");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Reflection overhead test completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Reflection calls made: {ITERATIONS * 4} (GetType + GetMethod + Invoke per iteration)");
        Console.WriteLine("Each reflection call has significant overhead");
    }
    
    static void Main() {
        Console.WriteLine("Starting reflection performance demonstration...");
        Console.WriteLine("Task: Calling methods repeatedly using reflection");
        Console.WriteLine("Monitor CPU Usage Tool for reflection overhead");
        Console.WriteLine();
        
        DemonstrateReflectionOverhead();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check CPU profiler for:");
        Console.WriteLine("- Time spent in Type.GetMethod()");
        Console.WriteLine("- Time spent in MethodInfo.Invoke()");
        Console.WriteLine("- Reflection infrastructure overhead");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR CACHED REFLECTION)
================================================================================

using System;
using System.Reflection;
using System.Diagnostics;
using System.Linq.Expressions;

class DataProcessor {
    public int ProcessValue(int value) {
        return value * value + 1;
    }
    
    public string ProcessString(string input) {
        return input?.ToUpper() + "_PROCESSED";
    }
}

class Program {
    // CORREÇÃO: Cache reflection objects to avoid repeated lookups
    private static readonly Type ProcessorType = typeof(DataProcessor);
    private static readonly MethodInfo ProcessValueMethod = ProcessorType.GetMethod("ProcessValue");
    private static readonly MethodInfo ProcessStringMethod = ProcessorType.GetMethod("ProcessString");
    
    static void DemonstrateCachedReflection() {
        Console.WriteLine("Starting cached reflection demonstration...");
        Console.WriteLine("Monitor CPU profiler - should see reduced reflection overhead");
        
        const int ITERATIONS = 100000;
        var processor = new DataProcessor();
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: Use cached MethodInfo - no repeated GetMethod() calls
            object result = ProcessValueMethod.Invoke(processor, new object[] { i });
            object stringResult = ProcessStringMethod.Invoke(processor, new object[] { $"Item_{i}" });
            
            if (i % 10000 == 0) {
                Console.WriteLine($"Cached reflection call {i}/{ITERATIONS}: {result}, {stringResult}");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Cached reflection completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine("Reflection metadata was cached - much faster!");
    }
    
    // CORREÇÃO: Even better - use compiled expressions for maximum performance
    private static readonly Func<DataProcessor, int, int> CompiledProcessValue;
    private static readonly Func<DataProcessor, string, string> CompiledProcessString;
    
    static Program() {
        // Compile expressions once at startup
        var processorParam = Expression.Parameter(typeof(DataProcessor), "processor");
        var valueParam = Expression.Parameter(typeof(int), "value");
        var stringParam = Expression.Parameter(typeof(string), "input");
        
        var processValueCall = Expression.Call(processorParam, ProcessValueMethod, valueParam);
        var processStringCall = Expression.Call(processorParam, ProcessStringMethod, stringParam);
        
        CompiledProcessValue = Expression.Lambda<Func<DataProcessor, int, int>>(
            processValueCall, processorParam, valueParam).Compile();
        
        CompiledProcessString = Expression.Lambda<Func<DataProcessor, string, string>>(
            processStringCall, processorParam, stringParam).Compile();
    }
    
    static void DemonstrateCompiledExpressions() {
        Console.WriteLine("Starting compiled expressions demonstration...");
        
        const int ITERATIONS = 100000;
        var processor = new DataProcessor();
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: Compiled expressions - nearly as fast as direct calls
            int result = CompiledProcessValue(processor, i);
            string stringResult = CompiledProcessString(processor, $"Item_{i}");
            
            if (i % 10000 == 0) {
                Console.WriteLine($"Compiled expression call {i}/{ITERATIONS}: {result}, {stringResult}");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Compiled expressions completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine("Compiled expressions provide near-native performance!");
    }
    
    static void DemonstrateDirectCalls() {
        Console.WriteLine("Starting direct calls for comparison...");
        
        const int ITERATIONS = 100000;
        var processor = new DataProcessor();
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < ITERATIONS; i++) {
            // Direct method calls - fastest possible
            int result = processor.ProcessValue(i);
            string stringResult = processor.ProcessString($"Item_{i}");
            
            if (i % 10000 == 0) {
                Console.WriteLine($"Direct call {i}/{ITERATIONS}: {result}, {stringResult}");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Direct calls completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine("Direct calls - baseline performance");
    }
    
    static void Main() {
        Console.WriteLine("Starting optimized reflection demonstration...");
        Console.WriteLine("Task: Comparing reflection optimization strategies");
        Console.WriteLine("Monitor CPU Usage Tool for performance improvements");
        Console.WriteLine();
        
        DemonstrateCachedReflection();
        Console.WriteLine();
        DemonstrateCompiledExpressions();
        Console.WriteLine();
        DemonstrateDirectCalls();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("Performance ranking (fastest to slowest):");
        Console.WriteLine("1. Direct calls - baseline");
        Console.WriteLine("2. Compiled expressions - ~10-20% overhead");
        Console.WriteLine("3. Cached reflection - ~100-500% overhead");
        Console.WriteLine("4. Uncached reflection - ~1000-5000% overhead");
    }
}

================================================================================
*/
