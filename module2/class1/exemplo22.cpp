/*
================================================================================
ATIVIDADE PRÁTICA 22 - COMPILER OPTIMIZATION INTERFERENCE (C++)
================================================================================

OBJETIVO:
- Demonstrar código que interfere com compiler optimizations
- Usar CPU profiler para identificar missed optimization opportunities
- Otimizar escrevendo compiler-friendly code
- Medir impacto de optimization barriers

PROBLEMA:
- Volatile keywords desnecessários
- Complex control flow que impede inlining
- CPU Profiler mostrará missed vectorization opportunities

SOLUÇÃO:
- Write optimization-friendly code
- Remove unnecessary volatile
- Simplify control flow para enable compiler optimizations

================================================================================
*/

#include <iostream>
#include <chrono>
#include <vector>
#include <random>
using namespace std;

// PERFORMANCE ISSUE: Volatile prevents compiler optimizations
volatile int global_counter = 0;

void demonstrateOptimizationInterference() {
    cout << "Starting compiler optimization interference demonstration..." << endl;
    cout << "Monitor CPU profiler - should see missed optimization opportunities" << endl;
    
    const int ARRAY_SIZE = 10000000;
    vector<double> data(ARRAY_SIZE);
    
    // Fill with random data
    random_device rd;
    mt19937 gen(rd());
    uniform_real_distribution<> dis(0.0, 100.0);
    
    for (int i = 0; i < ARRAY_SIZE; i++) {
        data[i] = dis(gen);
    }
    
    auto start = chrono::high_resolution_clock::now();
    
    double sum = 0.0;
    
    for (int i = 0; i < ARRAY_SIZE; i++) {
        // PERFORMANCE ISSUE: Volatile access prevents optimization
        global_counter++; // Compiler cannot optimize this away or reorder
        
        // PERFORMANCE ISSUE: Complex branching prevents vectorization
        if (data[i] > 50.0) {
            if (data[i] > 75.0) {
                sum += data[i] * 1.5;
            } else {
                sum += data[i] * 1.2;
            }
        } else {
            if (data[i] < 25.0) {
                sum += data[i] * 0.8;
            } else {
                sum += data[i] * 1.0;
            }
        }
        
        // PERFORMANCE ISSUE: Another volatile access
        volatile int temp = global_counter; // Prevents loop optimization
        
        if (i % 1000000 == 0) {
            cout << "Processed " << i << "/" << ARRAY_SIZE << " elements..." << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Optimization interference completed in: " << duration.count() << " ms" << endl;
    cout << "Sum: " << sum << endl;
    cout << "Global counter: " << global_counter << endl;
    cout << "Complex branching and volatile prevented compiler optimizations" << endl;
}

int main() {
    cout << "Starting compiler optimization interference demonstration..." << endl;
    cout << "Task: Processing array with optimization barriers" << endl;
    cout << "Monitor CPU Usage Tool for missed optimization opportunities" << endl << endl;
    
    demonstrateOptimizationInterference();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check CPU profiler for:" << endl;
    cout << "- Missed vectorization opportunities" << endl;
    cout << "- Memory access patterns with volatile" << endl;
    cout << "- Complex branching overhead" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR OPTIMIZATION-FRIENDLY CODE)
================================================================================

#include <iostream>
#include <chrono>
#include <vector>
#include <random>
using namespace std;

// CORREÇÃO: Remove volatile when not needed
int regular_counter = 0;

void demonstrateOptimizedCode() {
    cout << "Starting compiler optimization friendly demonstration..." << endl;
    cout << "Monitor CPU profiler - should see better optimization" << endl;
    
    const int ARRAY_SIZE = 10000000;
    vector<double> data(ARRAY_SIZE);
    
    // Fill with random data
    random_device rd;
    mt19937 gen(rd());
    uniform_real_distribution<> dis(0.0, 100.0);
    
    for (int i = 0; i < ARRAY_SIZE; i++) {
        data[i] = dis(gen);
    }
    
    auto start = chrono::high_resolution_clock::now();
    
    double sum = 0.0;
    int counter = 0; // CORREÇÃO: Local variable for better optimization
    
    // CORREÇÃO: Simplified logic that enables vectorization
    for (int i = 0; i < ARRAY_SIZE; i++) {
        counter++; // Local counter - compiler can optimize
        
        // CORREÇÃO: Simplified branching enables auto-vectorization
        double multiplier;
        if (data[i] > 75.0) {
            multiplier = 1.5;
        } else if (data[i] > 50.0) {
            multiplier = 1.2;
        } else if (data[i] < 25.0) {
            multiplier = 0.8;
        } else {
            multiplier = 1.0;
        }
        
        sum += data[i] * multiplier; // Can be vectorized
        
        if (i % 1000000 == 0) {
            cout << "Optimized processing: " << i << "/" << ARRAY_SIZE << " elements..." << endl;
        }
    }
    
    regular_counter = counter; // Single assignment at end
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Optimization friendly code completed in: " << duration.count() << " ms" << endl;
    cout << "Sum: " << sum << endl;
    cout << "Counter: " << regular_counter << endl;
    cout << "Simplified code enabled compiler optimizations" << endl;
}

// CORREÇÃO: Function that enables inlining
inline double computeMultiplier(double value) {
    if (value > 75.0) return 1.5;
    if (value > 50.0) return 1.2;  
    if (value < 25.0) return 0.8;
    return 1.0;
}

void demonstrateVectorizedLoop() {
    cout << "Starting vectorization-friendly demonstration..." << endl;
    
    const int ARRAY_SIZE = 10000000;
    vector<double> input(ARRAY_SIZE);
    vector<double> output(ARRAY_SIZE);
    
    // Initialize input
    for (int i = 0; i < ARRAY_SIZE; i++) {
        input[i] = i * 0.001;
    }
    
    auto start = chrono::high_resolution_clock::now();
    
    // CORREÇÃO: Simple loop that can be auto-vectorized by compiler
    for (int i = 0; i < ARRAY_SIZE; i++) {
        output[i] = input[i] * 2.0 + 1.0; // Perfect for SIMD vectorization
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Vectorizable loop completed in: " << duration.count() << " ms" << endl;
    cout << "Simple arithmetic enables SIMD vectorization" << endl;
}

void demonstrateLoopUnrolling() {
    cout << "Starting loop unrolling friendly demonstration..." << endl;
    
    const int ARRAY_SIZE = 10000000;
    vector<int> data(ARRAY_SIZE);
    
    for (int i = 0; i < ARRAY_SIZE; i++) {
        data[i] = i;
    }
    
    auto start = chrono::high_resolution_clock::now();
    
    long long sum = 0;
    
    // CORREÇÃO: Loop that can be unrolled effectively
    for (int i = 0; i < ARRAY_SIZE; i += 4) {
        // Manual unrolling hint for compiler
        sum += data[i];
        if (i + 1 < ARRAY_SIZE) sum += data[i + 1];
        if (i + 2 < ARRAY_SIZE) sum += data[i + 2]; 
        if (i + 3 < ARRAY_SIZE) sum += data[i + 3];
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Loop unrolling completed in: " << duration.count() << " ms" << endl;
    cout << "Sum: " << sum << endl;
    cout << "Unrolled loop reduces loop overhead" << endl;
}

int main() {
    cout << "Starting optimized compiler-friendly demonstration..." << endl;
    cout << "Task: Writing code that enables compiler optimizations" << endl;
    cout << "Monitor CPU Usage Tool for improved performance" << endl << endl;
    
    demonstrateOptimizedCode();
    cout << endl;
    demonstrateVectorizedLoop();
    cout << endl;
    demonstrateLoopUnrolling();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- Removed volatile enables optimization" << endl;
    cout << "- Simplified control flow enables vectorization" << endl;
    cout << "- Inline functions reduce call overhead" << endl;
    cout << "- Loop unrolling reduces branch overhead" << endl;
    cout << "- Compiler can apply SIMD and other optimizations" << endl;
    
    return 0;
}

================================================================================
*/
