/*
================================================================================
ATIVIDADE PRÁTICA 8 - EXCEPTION HANDLING PARA CONTROLE DE FLUXO (C#)
================================================================================

OBJETIVO:
- Demonstrar uso ineficiente de exceptions para controle de fluxo
- Usar CPU profiler para identificar overhead de exception handling
- Otimizar usando return types ou validation patterns
- Medir impacto das exceptions na performance

PROBLEMA:
- Exceptions são custosas quando usadas para controle de fluxo normal
- Exception handling envolve stack unwinding e cleanup
- CPU Profiler mostrará tempo gasto em exception handling

SOLUÇÃO:
- Usar exceptions apenas para erros excepcionais
- Implementar validação com return patterns ou Result<T>

================================================================================
*/

using System;
using System.Diagnostics;

class DataProcessor {
    // PERFORMANCE ISSUE: Using exceptions for normal control flow
    public int ProcessWithExceptions(int value) {
        if (value < 0) {
            throw new ArgumentException("Negative value not allowed"); // Exception for control flow - expensive!
        }
        if (value > 1000) {
            throw new ArgumentOutOfRangeException("Value too large"); // Another control flow exception
        }
        if (value % 2 == 0) {
            throw new InvalidOperationException("Even numbers not supported"); // Normal business logic as exception
        }
        
        return value * 2;
    }
}

class Program {
    static void DemonstrateExceptionOverhead() {
        Console.WriteLine("Starting exception-heavy processing...");
        Console.WriteLine("Monitor CPU profiler - should see overhead in exception handling");
        
        var processor = new DataProcessor();
        int successfulOperations = 0;
        int totalOperations = 100000;
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < totalOperations; i++) {
            try {
                int result = processor.ProcessWithExceptions(i % 1500);
                successfulOperations++;
            }
            catch (Exception ex) {
                // Exception handling overhead occurs here frequently
                continue; // Using exceptions for normal control flow
            }
            
            if (i % 10000 == 0) {
                Console.WriteLine($"Processed {i}/{totalOperations} values...");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Exception-based processing completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Successful operations: {successfulOperations}/{totalOperations}");
        Console.WriteLine($"Exceptions thrown: {totalOperations - successfulOperations}");
    }
    
    static void Main() {
        Console.WriteLine("Starting exception handling performance demonstration...");
        Console.WriteLine("Task: Processing data with exception-based validation");
        Console.WriteLine("Monitor CPU Usage Tool for exception handling overhead");
        Console.WriteLine();
        
        DemonstrateExceptionOverhead();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check CPU profiler for:");
        Console.WriteLine("- Time spent in exception construction/destruction");
        Console.WriteLine("- Stack unwinding overhead");
        Console.WriteLine("- Exception handler dispatch time");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR A VERSÃO OTIMIZADA)
================================================================================

using System;
using System.Diagnostics;

public enum ProcessResult {
    Success,
    NegativeValue,
    ValueTooLarge,
    EvenNumber
}

public class ProcessResultData {
    public ProcessResult Result { get; set; }
    public int? Value { get; set; }
    
    public static ProcessResultData Success(int value) => new() { Result = ProcessResult.Success, Value = value };
    public static ProcessResultData Failure(ProcessResult result) => new() { Result = result, Value = null };
}

class OptimizedDataProcessor {
    // CORREÇÃO: Using return codes instead of exceptions for control flow
    public ProcessResultData ProcessWithReturnCodes(int value) {
        if (value < 0) {
            return ProcessResultData.Failure(ProcessResult.NegativeValue); // Fast return - no exception overhead
        }
        if (value > 1000) {
            return ProcessResultData.Failure(ProcessResult.ValueTooLarge); // Fast return
        }
        if (value % 2 == 0) {
            return ProcessResultData.Failure(ProcessResult.EvenNumber); // Fast return
        }
        
        return ProcessResultData.Success(value * 2);
    }
}

class Program {
    static void DemonstrateOptimizedProcessing() {
        Console.WriteLine("Starting optimized processing...");
        Console.WriteLine("Monitor CPU profiler - should see reduced overhead");
        
        var processor = new OptimizedDataProcessor();
        int successfulOperations = 0;
        int totalOperations = 100000;
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < totalOperations; i++) {
            var result = processor.ProcessWithReturnCodes(i % 1500);
            
            if (result.Result == ProcessResult.Success && result.Value.HasValue) {
                successfulOperations++;
                // Process the successful result
            }
            // No exception handling overhead - just fast conditional checks
            
            if (i % 10000 == 0) {
                Console.WriteLine($"Processed {i}/{totalOperations} values...");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Optimized processing completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Successful operations: {successfulOperations}/{totalOperations}");
        Console.WriteLine("No exceptions thrown - using return codes for flow control");
    }
    
    static void Main() {
        Console.WriteLine("Starting optimized exception handling demonstration...");
        Console.WriteLine("Task: Processing data with return-code-based validation");
        Console.WriteLine("Monitor CPU Usage Tool for improved performance");
        Console.WriteLine();
        
        DemonstrateOptimizedProcessing();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("Improvements:");
        Console.WriteLine("- No exception construction/destruction overhead");
        Console.WriteLine("- No stack unwinding costs");
        Console.WriteLine("- Fast conditional logic for flow control");
    }
}

================================================================================
*/
