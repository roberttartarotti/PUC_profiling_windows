/*
================================================================================
ATIVIDADE PRÁTICA 6 - ANÁLISE E OTIMIZAÇÃO DE FUNÇÃO RECURSIVA (C#)
================================================================================

OBJETIVO:
- Implementar função recursiva ineficiente (Fibonacci)
- Encontrar gargalo no CPU Usage Tool
- Substituir por versão iterativa ou otimizar com memoization
- Comparar resultados e ganhos de CPU

PROBLEMA:
- Fibonacci recursivo possui complexidade O(2^n) - exponencial
- CPU Usage Tool mostrará que FibRecursive() domina tempo de CPU
- Fibonacci(42) faz ~2.7 bilhões de chamadas recursivas redundantes

SOLUÇÃO:
- Memoization: O(n) time, O(n) space - cache resultados intermediários
- Iterativo: O(n) time, O(1) space - mais eficiente em memória

================================================================================
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

class Program {
    private static long callCount = 0;
    
    static int FibRecursive(int n) {
        callCount++; // CPU HOTSPOT: Exponential O(2^n) recursive calls - Use memoization or iterative approach
        
        if (n <= 1) return n;
        return FibRecursive(n-1) + FibRecursive(n-2);
    }
    
    static long FibMemoization(int n, Dictionary<int, long> memo) {
        if (n <= 1) return n;
        
        if (memo.ContainsKey(n)) {
            return memo[n]; // SOLUTION: Memoization prevents redundant calculations, reduces to O(n)
        }
        
        memo[n] = FibMemoization(n-1, memo) + FibMemoization(n-2, memo);
        return memo[n];
    }
    
    static long FibIterative(int n) {
        if (n <= 1) return n;
        
        long prev2 = 0, prev1 = 1, current = 0;
        
        for (int i = 2; i <= n; i++) { // SOLUTION: Iterative approach O(n) time, O(1) space
            current = prev1 + prev2;
            prev2 = prev1;
            prev1 = current;
        }
        
        return current;
    }
    
    static void TestRecursiveFibonacci() {
        const int FIB_NUMBER = 42;
        Console.WriteLine("=== INEFFICIENT RECURSIVE FIBONACCI ===");
        Console.WriteLine($"Computing Fibonacci({FIB_NUMBER}) using naive recursion");
        Console.WriteLine("Monitor CPU Usage Tool - recursive calls will dominate CPU time");
        
        callCount = 0;
        var sw = Stopwatch.StartNew();
        
        long result = FibRecursive(FIB_NUMBER);
        
        sw.Stop();
        
        Console.WriteLine($"Result: {result}");
        Console.WriteLine($"Execution time: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Total recursive calls: {callCount} (exponential growth!)");
        Console.WriteLine("Time complexity: O(2^n) - extremely inefficient");
        Console.WriteLine();
    }
    
    static void TestMemoizedFibonacci() {
        const int FIB_NUMBER = 42;
        Console.WriteLine("=== OPTIMIZED MEMOIZED FIBONACCI ===");
        Console.WriteLine($"Computing Fibonacci({FIB_NUMBER}) using memoization");
        
        var memo = new Dictionary<int, long>();
        var sw = Stopwatch.StartNew();
        
        long result = FibMemoization(FIB_NUMBER, memo);
        
        sw.Stop();
        
        Console.WriteLine($"Result: {result}");
        Console.WriteLine($"Execution time: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Memoization table size: {memo.Count} entries");
        Console.WriteLine("Time complexity: O(n) - much more efficient!");
        Console.WriteLine();
    }
    
    static void TestIterativeFibonacci() {
        const int FIB_NUMBER = 42;
        Console.WriteLine("=== OPTIMIZED ITERATIVE FIBONACCI ===");
        Console.WriteLine($"Computing Fibonacci({FIB_NUMBER}) using iteration");
        
        var sw = Stopwatch.StartNew();
        
        long result = FibIterative(FIB_NUMBER);
        
        sw.Stop();
        
        Console.WriteLine($"Result: {result}");
        Console.WriteLine($"Execution time: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine("Space complexity: O(1) - most memory efficient!");
        Console.WriteLine("Time complexity: O(n) - linear time");
        Console.WriteLine();
    }
    
    static void Main() {
        Console.WriteLine("Starting recursive function analysis and optimization...");
        Console.WriteLine("Task: Computing Fibonacci numbers with different approaches");
        Console.WriteLine("Monitor CPU Usage Tool to identify recursive bottlenecks");
        Console.WriteLine();
        
        TestRecursiveFibonacci();
        
        Console.WriteLine("Waiting 2 seconds before next test...");
        Thread.Sleep(2000);
        
        TestMemoizedFibonacci();
        
        TestIterativeFibonacci();
        
        Console.WriteLine("=== PERFORMANCE COMPARISON ===");
        Console.WriteLine("- Recursive: O(2^n) time, massive CPU usage, exponential calls");
        Console.WriteLine("- Memoized: O(n) time, O(n) space, eliminates redundant calculations");
        Console.WriteLine("- Iterative: O(n) time, O(1) space, most efficient overall");
        Console.WriteLine("Expected speedup from recursive to optimized: 1000x+ improvement!");
    }
}

/*
================================================================================
OBSERVAÇÃO: Este exemplo já demonstra múltiplas abordagens
================================================================================

O código acima já inclui:
1. TestRecursiveFibonacci() - demonstra versão ineficiente O(2^n)
2. TestMemoizedFibonacci() - demonstra otimização com cache O(n)
3. TestIterativeFibonacci() - demonstra versão mais eficiente O(n)

Para foco apenas no problema:
- Comente as chamadas TestMemoizedFibonacci() e TestIterativeFibonacci()
- Execute apenas TestRecursiveFibonacci() para ver hotspot recursivo

Para foco apenas na solução:
- Comente a chamada TestRecursiveFibonacci()
- Execute apenas as versões otimizadas para comparar melhorias

================================================================================
*/