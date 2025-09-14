/*
================================================================================
ATIVIDADE PRÁTICA 16 - THREAD POOL STARVATION (C#)
================================================================================

OBJETIVO:
- Demonstrar thread pool starvation com blocking operations
- Usar CPU/Thread profiler para identificar thread pool exhaustion
- Otimizar usando async/await para non-blocking operations
- Medir impacto de thread pool starvation na escalabilidade

PROBLEMA:
- Blocking I/O operations consume thread pool threads
- Thread pool starvation impede novas tasks de executar
- Thread profiler mostrará all threads blocked

SOLUÇÃO:
- Usar async I/O para liberar threads durante waits
- ConfigureAwait(false) para library code

================================================================================
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;

class Program {
    static void DemonstrateThreadPoolStarvation() {
        Console.WriteLine("Starting thread pool starvation demonstration...");
        Console.WriteLine("Monitor thread profiler - should see thread pool exhaustion");
        
        const int CONCURRENT_OPERATIONS = 50;
        int initialThreads = ThreadPool.ThreadCount;
        
        var sw = Stopwatch.StartNew();
        
        // PERFORMANCE ISSUE: Blocking operations that tie up thread pool threads
        var tasks = new Task[CONCURRENT_OPERATIONS];
        
        for (int i = 0; i < CONCURRENT_OPERATIONS; i++) {
            int taskId = i;
            tasks[i] = Task.Run(() => {
                // PERFORMANCE ISSUE: Synchronous blocking call ties up thread
                Thread.Sleep(2000); // Blocking sleep consumes thread pool thread
                
                Console.WriteLine($"Task {taskId} completed on thread {Thread.CurrentThread.ManagedThreadId}");
                return taskId;
            });
        }
        
        // Wait for all tasks - but thread pool may be starved
        Task.WaitAll(tasks);
        
        sw.Stop();
        
        Console.WriteLine($"Thread pool starvation test completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Initial threads: {initialThreads}");
        Console.WriteLine($"Final thread count: {ThreadPool.ThreadCount}");
        Console.WriteLine($"Tasks executed: {CONCURRENT_OPERATIONS}");
        Console.WriteLine("All thread pool threads were blocked during wait periods");
    }
    
    static void Main() {
        // Configure thread pool to show starvation more clearly
        ThreadPool.SetMaxThreads(10, 10);
        ThreadPool.SetMinThreads(2, 2);
        
        Console.WriteLine("Starting thread pool starvation demonstration...");
        Console.WriteLine("Task: Running many blocking operations concurrently");
        Console.WriteLine("Monitor Thread Usage and CPU profiler for thread pool issues");
        Console.WriteLine();
        
        DemonstrateThreadPoolStarvation();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check profiler for:");
        Console.WriteLine("- Thread pool thread exhaustion");
        Console.WriteLine("- Blocked threads during I/O waits");
        Console.WriteLine("- Poor thread utilization");
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
    static async Task DemonstrateAsyncOperations() {
        Console.WriteLine("Starting async operations demonstration...");
        Console.WriteLine("Monitor thread profiler - should see efficient thread usage");
        
        const int CONCURRENT_OPERATIONS = 50;
        int initialThreads = ThreadPool.ThreadCount;
        
        var sw = Stopwatch.StartNew();
        
        // CORREÇÃO: Async operations that release threads during waits
        var tasks = new Task<int>[CONCURRENT_OPERATIONS];
        
        for (int i = 0; i < CONCURRENT_OPERATIONS; i++) {
            int taskId = i;
            tasks[i] = PerformAsyncOperation(taskId);
        }
        
        // CORREÇÃO: Async wait that doesn't block threads
        var results = await Task.WhenAll(tasks);
        
        sw.Stop();
        
        Console.WriteLine($"Async operations completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Initial threads: {initialThreads}");
        Console.WriteLine($"Final thread count: {ThreadPool.ThreadCount}");
        Console.WriteLine($"Tasks executed: {CONCURRENT_OPERATIONS}");
        Console.WriteLine("Threads were returned to pool during async waits");
    }
    
    static async Task<int> PerformAsyncOperation(int taskId) {
        // CORREÇÃO: Async delay releases the thread back to pool
        await Task.Delay(2000).ConfigureAwait(false); // Non-blocking wait
        
        Console.WriteLine($"Task {taskId} completed on thread {Thread.CurrentThread.ManagedThreadId}");
        return taskId;
    }
    
    // Example with real async I/O
    static async Task DemonstrateAsyncIO() {
        Console.WriteLine("Starting async I/O demonstration...");
        
        const int CONCURRENT_REQUESTS = 20;
        using var httpClient = new HttpClient();
        
        var sw = Stopwatch.StartNew();
        
        // CORREÇÃO: Async HTTP calls that don't block threads
        var tasks = new Task<string>[CONCURRENT_REQUESTS];
        
        for (int i = 0; i < CONCURRENT_REQUESTS; i++) {
            tasks[i] = httpClient.GetStringAsync($"https://httpbin.org/delay/1");
        }
        
        var responses = await Task.WhenAll(tasks);
        
        sw.Stop();
        
        Console.WriteLine($"Async I/O completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Concurrent requests: {CONCURRENT_REQUESTS}");
        Console.WriteLine($"Average response length: {responses.Average(r => r.Length):F0} chars");
        Console.WriteLine("HTTP I/O was non-blocking - threads returned to pool");
    }
    
    static async Task Main() {
        // Configure thread pool to show the difference
        ThreadPool.SetMaxThreads(10, 10);
        ThreadPool.SetMinThreads(2, 2);
        
        Console.WriteLine("Starting optimized async demonstration...");
        Console.WriteLine("Task: Using async/await for concurrent operations");
        Console.WriteLine("Monitor Thread Usage for efficient thread utilization");
        Console.WriteLine();
        
        await DemonstrateAsyncOperations();
        Console.WriteLine();
        await DemonstrateAsyncIO();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("Improvements:");
        Console.WriteLine("- Threads returned to pool during async waits");
        Console.WriteLine("- No thread pool starvation");
        Console.WriteLine("- Better scalability for I/O-bound operations");
        Console.WriteLine("- More efficient use of system resources");
    }
}

================================================================================
*/
