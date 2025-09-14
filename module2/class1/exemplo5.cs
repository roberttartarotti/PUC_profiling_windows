/*
================================================================================
ATIVIDADE PRÁTICA 5 - PARALELIZAÇÃO E USO DE NÚCLEOS DA CPU (C#)
================================================================================

OBJETIVO:
- Implementar processamento sequencial usando loop tradicional
- Refatorar usando paralelismo (Parallel.ForEach)
- Usar CPU Usage Tool para analisar distribuição do uso de CPU
- Medir ganho de desempenho com utilização de múltiplos núcleos

PROBLEMA:
- Processamento sequencial usa apenas 1 núcleo de CPU (~100% de 1 core)
- Outros núcleos ficam ociosos, desperdiçando capacidade de processamento
- CPU Usage Tool mostrará uso de single-core

SOLUÇÃO:
- Implementar paralelização com Parallel.ForEach para utilizar todos os núcleos
- Resultado: distribuição de carga across todos os cores disponíveis

================================================================================
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

class Program {
    static bool IsPrime(long n) {
        if (n <= 1) return false;
        if (n <= 3) return true;
        if (n % 2 == 0 || n % 3 == 0) return false;
        
        for (long i = 5; i * i <= n; i += 6) {
            if (n % i == 0 || n % (i + 2) == 0) {
                return false;
            }
        }
        return true;
    }
    
    static void SequentialProcessing(List<long> numbers) {
        Console.WriteLine("Starting SEQUENTIAL processing...");
        Console.WriteLine("CPU Usage Tool should show single-core utilization");
        
        var sw = Stopwatch.StartNew();
        int primeCount = 0;
        
        for (int i = 0; i < numbers.Count; i++) {
            if (IsPrime(numbers[i])) { // CPU INTENSIVE: Single-threaded processing - Use Parallel.ForEach for multi-core utilization
                primeCount++;
            }
            
            if (i % 1000 == 0) {
                Console.WriteLine($"Sequential progress: {i}/{numbers.Count} numbers processed");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine("=== SEQUENTIAL RESULTS ===");
        Console.WriteLine($"Execution time: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Primes found: {primeCount}");
        Console.WriteLine("CPU cores used: 1 (sequential processing)");
        Console.WriteLine();
    }
    
    static void ParallelProcessing(List<long> numbers) {
        Console.WriteLine("Starting PARALLEL processing...");
        Console.WriteLine("CPU Usage Tool should show multi-core utilization");
        
        var sw = Stopwatch.StartNew();
        object lockObj = new object();
        int primeCount = 0;
        int processed = 0;
        
        int coreCount = Environment.ProcessorCount;
        Console.WriteLine($"Using {coreCount} cores for parallel processing");
        
        Parallel.ForEach(numbers, number => {
            if (IsPrime(number)) { // SOLUTION: Parallel.ForEach utilizes all CPU cores automatically
                lock (lockObj) {
                    primeCount++;
                }
            }
            
            int current = Interlocked.Increment(ref processed);
            if (current % 1000 == 0) {
                Console.WriteLine($"Parallel progress: {current}/{numbers.Count} numbers processed");
            }
        });
        
        sw.Stop();
        
        Console.WriteLine("=== PARALLEL RESULTS ===");
        Console.WriteLine($"Execution time: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Primes found: {primeCount}");
        Console.WriteLine($"CPU cores used: {coreCount} (parallel processing)");
        Console.WriteLine();
    }
    
    static void Main() {
        const int DATA_SIZE = 50000;
        Console.WriteLine("Starting CPU parallelization demonstration...");
        Console.WriteLine("Task: Finding prime numbers in range");
        Console.WriteLine($"Data size: {DATA_SIZE} numbers");
        Console.WriteLine("Monitor CPU Usage Tool to see single-core vs multi-core utilization");
        Console.WriteLine();
        
        var numbers = new List<long>();
        for (int i = 0; i < DATA_SIZE; i++) {
            numbers.Add(100000 + i);
        }
        
        Console.WriteLine($"Test data generated (numbers from 100000 to {100000 + DATA_SIZE - 1})");
        Console.WriteLine();
        
        SequentialProcessing(numbers);
        
        Thread.Sleep(2000);
        
        ParallelProcessing(numbers);
        
        Console.WriteLine("=== PERFORMANCE COMPARISON ===");
        Console.WriteLine("Compare the execution times and CPU usage patterns:");
        Console.WriteLine("- Sequential: Uses 1 CPU core at ~100%");
        Console.WriteLine("- Parallel: Distributes load across all available cores");
        Console.WriteLine($"- Expected speedup: ~{Environment.ProcessorCount}x (ideal case)");
    }
}

/*
================================================================================
OBSERVAÇÃO: Este exemplo já demonstra ambas as abordagens (sequencial vs paralela)
================================================================================

O código acima já inclui:
1. SequentialProcessing() - demonstra processamento single-threaded
2. ParallelProcessing() - demonstra processamento multi-threaded otimizado

Para foco apenas no problema:
- Comente a chamada ParallelProcessing() no Main()
- Execute apenas SequentialProcessing() para ver uso de single-core

Para foco apenas na solução:
- Comente a chamada SequentialProcessing() no Main()  
- Execute apenas ParallelProcessing() para ver uso multi-core

================================================================================
*/