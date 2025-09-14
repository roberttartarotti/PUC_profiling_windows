/*
================================================================================
ATIVIDADE PRÁTICA 15 - CACHE MISS PERFORMANCE (C++)
================================================================================

OBJETIVO:
- Demonstrar impacto de cache misses em performance
- Usar CPU profiler para identificar cache-unfriendly memory access patterns
- Otimizar melhorando data locality e access patterns
- Comparar row-major vs column-major matrix traversal

PROBLEMA:
- Acesso não-sequencial à memória causa cache misses
- Column-major traversal em row-major data é cache-unfriendly
- CPU profiler mostrará alto cache miss ratio

SOLUÇÃO:
- Usar data layout que favoreça cache locality
- Sequential memory access patterns

================================================================================
*/

#include <iostream>
#include <chrono>
#include <vector>
#include <random>
using namespace std;

void demonstrateCacheMisses() {
    cout << "Starting cache miss demonstration..." << endl;
    cout << "Monitor CPU profiler - should see high cache miss ratio" << endl;
    
    const int MATRIX_SIZE = 2000;
    const int ITERATIONS = 3;
    
    // Allocate large matrix
    vector<vector<int>> matrix(MATRIX_SIZE, vector<int>(MATRIX_SIZE));
    
    // Initialize matrix
    for (int i = 0; i < MATRIX_SIZE; i++) {
        for (int j = 0; j < MATRIX_SIZE; j++) {
            matrix[i][j] = i * j;
        }
    }
    
    auto start = chrono::high_resolution_clock::now();
    
    long long sum = 0;
    
    // PERFORMANCE ISSUE: Column-major traversal of row-major data
    // This creates cache misses because we're jumping between cache lines
    for (int iter = 0; iter < ITERATIONS; iter++) {
        for (int j = 0; j < MATRIX_SIZE; j++) {        // Column first
            for (int i = 0; i < MATRIX_SIZE; i++) {    // Row second - BAD for cache
                sum += matrix[i][j]; // Cache miss - different cache line each access
            }
        }
        
        if (iter == 0) {
            cout << "Completed iteration " << (iter + 1) << "/" << ITERATIONS 
                 << " (cache-unfriendly access pattern)" << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Cache-unfriendly traversal completed in: " << duration.count() << " ms" << endl;
    cout << "Sum: " << sum << " (to prevent optimization)" << endl;
    cout << "Access pattern: Column-major on row-major data (cache misses)" << endl;
}

int main() {
    cout << "Starting cache performance demonstration..." << endl;
    cout << "Task: Matrix traversal with cache-unfriendly access pattern" << endl;
    cout << "Monitor CPU Usage Tool and cache performance counters" << endl << endl;
    
    demonstrateCacheMisses();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check CPU profiler for:" << endl;
    cout << "- High cache miss ratio" << endl;
    cout << "- Memory stall cycles" << endl;
    cout << "- Poor memory bandwidth utilization" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR CACHE-FRIENDLY ACCESS)
================================================================================

#include <iostream>
#include <chrono>
#include <vector>
using namespace std;

void demonstrateCacheFriendly() {
    cout << "Starting cache-friendly demonstration..." << endl;
    cout << "Monitor CPU profiler - should see improved cache hit ratio" << endl;
    
    const int MATRIX_SIZE = 2000;
    const int ITERATIONS = 3;
    
    // CORREÇÃO: Use single-dimensional array for better cache locality
    vector<int> matrix(MATRIX_SIZE * MATRIX_SIZE);
    
    // Initialize matrix
    for (int i = 0; i < MATRIX_SIZE; i++) {
        for (int j = 0; j < MATRIX_SIZE; j++) {
            matrix[i * MATRIX_SIZE + j] = i * j;
        }
    }
    
    auto start = chrono::high_resolution_clock::now();
    
    long long sum = 0;
    
    // CORREÇÃO: Row-major traversal matches data layout - cache friendly
    for (int iter = 0; iter < ITERATIONS; iter++) {
        for (int i = 0; i < MATRIX_SIZE; i++) {        // Row first
            for (int j = 0; j < MATRIX_SIZE; j++) {    // Column second - GOOD for cache
                sum += matrix[i * MATRIX_SIZE + j]; // Cache hit - sequential access
            }
        }
        
        if (iter == 0) {
            cout << "Completed iteration " << (iter + 1) << "/" << ITERATIONS 
                 << " (cache-friendly access pattern)" << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Cache-friendly traversal completed in: " << duration.count() << " ms" << endl;
    cout << "Sum: " << sum << " (to prevent optimization)" << endl;
    cout << "Access pattern: Row-major sequential (cache hits)" << endl;
}

void demonstrateBlockedTraversal() {
    cout << "Starting cache-optimized blocked traversal..." << endl;
    
    const int MATRIX_SIZE = 2000;
    const int BLOCK_SIZE = 64; // Cache line friendly block size
    
    vector<int> matrix(MATRIX_SIZE * MATRIX_SIZE);
    
    // Initialize
    for (int i = 0; i < MATRIX_SIZE; i++) {
        for (int j = 0; j < MATRIX_SIZE; j++) {
            matrix[i * MATRIX_SIZE + j] = i + j;
        }
    }
    
    auto start = chrono::high_resolution_clock::now();
    
    long long sum = 0;
    
    // CORREÇÃO: Blocked traversal for even better cache utilization
    for (int bi = 0; bi < MATRIX_SIZE; bi += BLOCK_SIZE) {
        for (int bj = 0; bj < MATRIX_SIZE; bj += BLOCK_SIZE) {
            // Process block
            for (int i = bi; i < min(bi + BLOCK_SIZE, MATRIX_SIZE); i++) {
                for (int j = bj; j < min(bj + BLOCK_SIZE, MATRIX_SIZE); j++) {
                    sum += matrix[i * MATRIX_SIZE + j];
                }
            }
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Blocked traversal completed in: " << duration.count() << " ms" << endl;
    cout << "Sum: " << sum << endl;
}

int main() {
    cout << "Starting optimized cache performance demonstration..." << endl;
    cout << "Task: Matrix traversal with cache-friendly patterns" << endl;
    cout << "Monitor CPU Usage Tool for improved cache performance" << endl << endl;
    
    demonstrateCacheFriendly();
    cout << endl;
    demonstrateBlockedTraversal();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- Sequential memory access improves cache hit ratio" << endl;
    cout << "- Blocked traversal maximizes cache line utilization" << endl;
    cout << "- Better memory bandwidth utilization" << endl;
    cout << "- Significantly faster execution time" << endl;
    
    return 0;
}

================================================================================
*/
