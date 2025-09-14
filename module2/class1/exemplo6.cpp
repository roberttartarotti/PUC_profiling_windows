/*
================================================================================
ATIVIDADE PRÁTICA 6 - ANÁLISE E OTIMIZAÇÃO DE FUNÇÃO RECURSIVA (C++)
================================================================================

OBJETIVO:
- Implementar função recursiva ineficiente (Fibonacci)
- Encontrar gargalo no CPU Usage Tool
- Substituir por versão iterativa ou otimizar com memoization
- Comparar resultados e ganhos de CPU

PROBLEMA:
- Fibonacci recursivo possui complexidade O(2^n) - exponencial
- CPU Usage Tool mostrará que fibRecursive() domina tempo de CPU
- Fibonacci(42) faz ~2.7 bilhões de chamadas recursivas redundantes

SOLUÇÃO:
- Memoization: O(n) time, O(n) space - cache resultados intermediários
- Iterativo: O(n) time, O(1) space - mais eficiente em memória

================================================================================
*/

#include <iostream>
#include <chrono>
#include <unordered_map>
#include <thread>
using namespace std;

long long callCount = 0;

int fibRecursive(int n) {
    callCount++; // CPU HOTSPOT: Exponential O(2^n) recursive calls - Use memoization or iterative approach
    
    if (n <= 1) return n;
    return fibRecursive(n-1) + fibRecursive(n-2);
}

int fibMemoization(int n, unordered_map<int, long long>& memo) {
    if (n <= 1) return n;
    
    if (memo.find(n) != memo.end()) {
        return memo[n]; // SOLUTION: Memoization prevents redundant calculations, reduces to O(n)
    }
    
    memo[n] = fibMemoization(n-1, memo) + fibMemoization(n-2, memo);
    return memo[n];
}

long long fibIterative(int n) {
    if (n <= 1) return n;
    
    long long prev2 = 0, prev1 = 1, current = 0;
    
    for (int i = 2; i <= n; i++) { // SOLUTION: Iterative approach O(n) time, O(1) space
        current = prev1 + prev2;
        prev2 = prev1;
        prev1 = current;
    }
    
    return current;
}

void testRecursiveFibonacci() {
    const int FIB_NUMBER = 42;
    cout << "=== INEFFICIENT RECURSIVE FIBONACCI ===" << endl;
    cout << "Computing Fibonacci(" << FIB_NUMBER << ") using naive recursion" << endl;
    cout << "Monitor CPU Usage Tool - recursive calls will dominate CPU time" << endl;
    
    callCount = 0;
    auto start = chrono::high_resolution_clock::now();
    
    long long result = fibRecursive(FIB_NUMBER);
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Result: " << result << endl;
    cout << "Execution time: " << duration.count() << " ms" << endl;
    cout << "Total recursive calls: " << callCount << " (exponential growth!)" << endl;
    cout << "Time complexity: O(2^n) - extremely inefficient" << endl << endl;
}

void testMemoizedFibonacci() {
    const int FIB_NUMBER = 42;
    cout << "=== OPTIMIZED MEMOIZED FIBONACCI ===" << endl;
    cout << "Computing Fibonacci(" << FIB_NUMBER << ") using memoization" << endl;
    
    unordered_map<int, long long> memo;
    auto start = chrono::high_resolution_clock::now();
    
    long long result = fibMemoization(FIB_NUMBER, memo);
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Result: " << result << endl;
    cout << "Execution time: " << duration.count() << " ms" << endl;
    cout << "Memoization table size: " << memo.size() << " entries" << endl;
    cout << "Time complexity: O(n) - much more efficient!" << endl << endl;
}

void testIterativeFibonacci() {
    const int FIB_NUMBER = 42;
    cout << "=== OPTIMIZED ITERATIVE FIBONACCI ===" << endl;
    cout << "Computing Fibonacci(" << FIB_NUMBER << ") using iteration" << endl;
    
    auto start = chrono::high_resolution_clock::now();
    
    long long result = fibIterative(FIB_NUMBER);
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Result: " << result << endl;
    cout << "Execution time: " << duration.count() << " ms" << endl;
    cout << "Space complexity: O(1) - most memory efficient!" << endl;
    cout << "Time complexity: O(n) - linear time" << endl << endl;
}

int main() {
    cout << "Starting recursive function analysis and optimization..." << endl;
    cout << "Task: Computing Fibonacci numbers with different approaches" << endl;
    cout << "Monitor CPU Usage Tool to identify recursive bottlenecks" << endl << endl;
    
    testRecursiveFibonacci();
    
    cout << "Waiting 2 seconds before next test..." << endl;
    this_thread::sleep_for(chrono::seconds(2));
    
    testMemoizedFibonacci();
    
    testIterativeFibonacci();
    
    cout << "=== PERFORMANCE COMPARISON ===" << endl;
    cout << "- Recursive: O(2^n) time, massive CPU usage, exponential calls" << endl;
    cout << "- Memoized: O(n) time, O(n) space, eliminates redundant calculations" << endl;  
    cout << "- Iterative: O(n) time, O(1) space, most efficient overall" << endl;
    cout << "Expected speedup from recursive to optimized: 1000x+ improvement!" << endl;
    
    return 0;
}

/*
================================================================================
OBSERVAÇÃO: Este exemplo já demonstra múltiplas abordagens
================================================================================

O código acima já inclui:
1. testRecursiveFibonacci() - demonstra versão ineficiente O(2^n)
2. testMemoizedFibonacci() - demonstra otimização com cache O(n)
3. testIterativeFibonacci() - demonstra versão mais eficiente O(n)

Para foco apenas no problema:
- Comente as chamadas testMemoizedFibonacci() e testIterativeFibonacci()
- Execute apenas testRecursiveFibonacci() para ver hotspot recursivo

Para foco apenas na solução:
- Comente a chamada testRecursiveFibonacci()
- Execute apenas as versões otimizadas para comparar melhorias

================================================================================
*/