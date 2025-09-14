/*
================================================================================
ATIVIDADE PRÁTICA 13 - BUSY WAITING VS ASYNC/AWAIT (C#)
================================================================================

OBJETIVO:
- Demonstrar ineficiência de busy waiting vs async programming
- Usar CPU profiler para identificar wasted CPU cycles em polling
- Otimizar usando async/await e TaskCompletionSource
- Comparar thread utilization entre sync e async approaches

PROBLEMA:
- Busy waiting bloqueia threads e desperdiça CPU
- Thread.Sleep em loops ainda consome thread pool threads
- CPU Profiler mostrará threads bloqueadas

SOLUÇÃO:
- Usar async/await para non-blocking operations
- TaskCompletionSource para event-driven async code

================================================================================
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

class Program {
    private static volatile bool dataReady = false;
    private static int pollingAttempts = 0;
    
    static void DataProducer() {
        Thread.Sleep(3000); // Simulate work
        dataReady = true;
        Console.WriteLine("Data producer: Data is ready!");
    }
    
    static void DemonstrateBusyWaiting() {
        Console.WriteLine("Starting busy waiting demonstration...");
        Console.WriteLine("Monitor CPU profiler - should see wasted CPU cycles and blocked threads");
        
        var producer = new Thread(DataProducer);
        producer.Start();
        
        var sw = Stopwatch.StartNew();
        
        // PERFORMANCE ISSUE: Busy waiting blocks thread and wastes CPU
        while (!dataReady) {
            pollingAttempts++;
            Thread.Sleep(1); // Still blocks thread even with sleep
            
            if (pollingAttempts % 1000 == 0) {
                Console.WriteLine($"Busy waiting... polled {pollingAttempts} times");
                Console.WriteLine($"Active threads: {Process.GetCurrentProcess().Threads.Count}");
            }
        }
        
        sw.Stop();
        producer.Join();
        
        Console.WriteLine($"Busy waiting completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Total polling attempts: {pollingAttempts}");
        Console.WriteLine("Thread was blocked during entire wait period");
    }
    
    static void Main() {
        Console.WriteLine("Starting busy waiting vs async demonstration...");
        Console.WriteLine("Task: Waiting for data to become available");
        Console.WriteLine("Monitor CPU Usage Tool and thread utilization");
        Console.WriteLine();
        
        DemonstrateBusyWaiting();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check profiler for:");
        Console.WriteLine("- Blocked threads during wait period");
        Console.WriteLine("- Wasted CPU cycles in polling loop");
        Console.WriteLine("- Poor thread pool utilization");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR ASYNC/AWAIT)
================================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

class Program {
    private static TaskCompletionSource<bool> dataReadyTcs = new TaskCompletionSource<bool>();
    
    static async Task DataProducerAsync() {
        await Task.Delay(3000); // Simulate async work
        dataReadyTcs.SetResult(true);
        Console.WriteLine("Async producer: Data is ready, completing task!");
    }
    
    static async Task DemonstrateAsyncWait() {
        Console.WriteLine("Starting async/await demonstration...");
        Console.WriteLine("Monitor CPU profiler - should see efficient thread utilization");
        
        var producerTask = DataProducerAsync();
        
        var sw = Stopwatch.StartNew();
        
        // CORREÇÃO: Async wait - no thread blocking, no CPU waste
        await dataReadyTcs.Task; // Efficient async wait
        
        sw.Stop();
        await producerTask;
        
        Console.WriteLine($"Async wait completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine("No polling attempts - pure event-driven");
        Console.WriteLine("Thread was returned to pool during wait");
        Console.WriteLine($"Active threads: {Process.GetCurrentProcess().Threads.Count}");
    }
    
    static async Task Main() {
        Console.WriteLine("Starting optimized async/await demonstration...");
        Console.WriteLine("Task: Waiting efficiently using async patterns");
        Console.WriteLine("Monitor CPU Usage Tool for improved thread utilization");
        Console.WriteLine();
        
        await DemonstrateAsyncWait();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("Improvements:");
        Console.WriteLine("- No CPU wasted during wait period");
        Console.WriteLine("- Thread returned to pool for other work");
        Console.WriteLine("- Event-driven completion");
        Console.WriteLine("- Better scalability for concurrent operations");
        Console.WriteLine("- Improved thread pool utilization");
    }
}

================================================================================
*/
