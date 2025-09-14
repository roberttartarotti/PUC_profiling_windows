/*
================================================================================
ATIVIDADE PRÁTICA 26 - BRANCH PREDICTION FAILURES (C++)
================================================================================

OBJETIVO:
- Demonstrar impacto de branch misprediction na performance
- Usar CPU profiler para identificar branch prediction miss penalties
- Otimizar usando predictable branching patterns
- Medir diferença entre random vs predictable branches

PROBLEMA:
- Random branching causes pipeline stalls
- Branch misprediction penalties
- CPU Profiler mostrará branch miss statistics

SOLUÇÃO:
- Reorganizar código para predictable branches
- Use branchless programming quando apropriado
- Sort data para improve branch predictability

================================================================================
*/

#include <iostream>
#include <chrono>
#include <vector>
#include <random>
#include <algorithm>
using namespace std;

void demonstrateUnpredictableBranching() {
    cout << "Starting unpredictable branching demonstration..." << endl;
    cout << "Monitor CPU profiler - should see branch misprediction penalties" << endl;
    
    const int DATA_SIZE = 10000000;
    vector<int> data(DATA_SIZE);
    
    // Fill with random data
    random_device rd;
    mt19937 gen(rd());
    uniform_int_distribution<> dis(0, 255);
    
    for (int i = 0; i < DATA_SIZE; i++) {
        data[i] = dis(gen);
    }
    
    auto start = chrono::high_resolution_clock::now();
    
    long long sum = 0;
    int threshold = 128;
    
    // PERFORMANCE ISSUE: Random branching causes mispredictions
    for (int i = 0; i < DATA_SIZE; i++) {
        if (data[i] >= threshold) {  // Unpredictable branch - 50/50 chance
            sum += data[i];          // This branch is hard to predict
        } else {
            sum -= data[i];          // Random pattern causes pipeline stalls
        }
        
        if (i % 1000000 == 0) {
            cout << "Unpredictable branching: " << i << "/" << DATA_SIZE << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Unpredictable branching completed in: " << duration.count() << " ms" << endl;
    cout << "Sum: " << sum << endl;
    cout << "Random branch pattern caused many mispredictions" << endl;
}

int main() {
    cout << "Starting branch prediction performance demonstration..." << endl;
    cout << "Task: Processing data with unpredictable branching patterns" << endl;
    cout << "Monitor CPU Usage Tool for branch misprediction overhead" << endl << endl;
    
    demonstrateUnpredictableBranching();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check CPU profiler for:" << endl;
    cout << "- Branch misprediction statistics" << endl;
    cout << "- Pipeline stall cycles" << endl;
    cout << "- Instructions per cycle degradation" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR PREDICTABLE BRANCHES)
================================================================================

#include <iostream>
#include <chrono>
#include <vector>
#include <random>
#include <algorithm>
using namespace std;

void demonstratePredictableBranching() {
    cout << "Starting predictable branching demonstration..." << endl;
    cout << "Monitor CPU profiler - should see improved branch prediction" << endl;
    
    const int DATA_SIZE = 10000000;
    vector<int> data(DATA_SIZE);
    
    // Fill with random data
    random_device rd;
    mt19937 gen(rd());
    uniform_int_distribution<> dis(0, 255);
    
    for (int i = 0; i < DATA_SIZE; i++) {
        data[i] = dis(gen);
    }
    
    // CORREÇÃO: Sort data to make branches predictable
    sort(data.begin(), data.end());
    
    auto start = chrono::high_resolution_clock::now();
    
    long long sum = 0;
    int threshold = 128;
    
    // CORREÇÃO: Predictable branching pattern after sorting
    for (int i = 0; i < DATA_SIZE; i++) {
        if (data[i] >= threshold) {  // Predictable - all small values first, then large
            sum += data[i];          // Branch predictor learns pattern quickly
        } else {
            sum -= data[i];          // Very predictable due to sorted data
        }
        
        if (i % 1000000 == 0) {
            cout << "Predictable branching: " << i << "/" << DATA_SIZE << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Predictable branching completed in: " << duration.count() << " ms" << endl;
    cout << "Sum: " << sum << endl;
    cout << "Sorted data enabled predictable branch patterns" << endl;
}

void demonstrateBranchlessCode() {
    cout << "Starting branchless code demonstration..." << endl;
    
    const int DATA_SIZE = 10000000;
    vector<int> data(DATA_SIZE);
    
    random_device rd;
    mt19937 gen(rd());
    uniform_int_distribution<> dis(0, 255);
    
    for (int i = 0; i < DATA_SIZE; i++) {
        data[i] = dis(gen);
    }
    
    auto start = chrono::high_resolution_clock::now();
    
    long long sum = 0;
    int threshold = 128;
    
    // CORREÇÃO: Branchless programming eliminates mispredictions
    for (int i = 0; i < DATA_SIZE; i++) {
        // Branchless conditional using arithmetic
        int condition = (data[i] >= threshold);  // 0 or 1
        sum += condition * data[i];              // Add if >= threshold
        sum -= (1 - condition) * data[i];       // Subtract if < threshold
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Branchless code completed in: " << duration.count() << " ms" << endl;
    cout << "Sum: " << sum << endl;
    cout << "No branches = no mispredictions" << endl;
}

void demonstrateOptimizedBranchLayout() {
    cout << "Starting optimized branch layout demonstration..." << endl;
    
    const int DATA_SIZE = 5000000;
    vector<int> data(DATA_SIZE);
    
    // Create data with known distribution for optimization
    for (int i = 0; i < DATA_SIZE; i++) {
        data[i] = i % 1000; // Predictable pattern
    }
    
    auto start = chrono::high_resolution_clock::now();
    
    long long sum = 0;
    
    // CORREÇÃO: Optimize branch layout for common case
    for (int i = 0; i < DATA_SIZE; i++) {
        // Most values will be < 800, so optimize for this path
        if (data[i] < 800) {  // Common case first - better branch prediction
            sum += data[i] * 2;
        } else if (data[i] < 900) {  // Less common case
            sum += data[i] * 3;
        } else {  // Rare case last
            sum += data[i] * 5;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Optimized branch layout completed in: " << duration.count() << " ms" << endl;
    cout << "Sum: " << sum << endl;
    cout << "Common-case-first layout improved prediction" << endl;
}

void demonstrateConditionalMove() {
    cout << "Starting conditional move demonstration..." << endl;
    
    const int DATA_SIZE = 10000000;
    vector<int> data(DATA_SIZE);
    
    random_device rd;
    mt19937 gen(rd());
    uniform_int_distribution<> dis(0, 1000);
    
    for (int i = 0; i < DATA_SIZE; i++) {
        data[i] = dis(gen);
    }
    
    auto start = chrono::high_resolution_clock::now();
    
    // CORREÇÃO: Using conditional move (cmov) instead of branches
    for (int i = 0; i < DATA_SIZE; i++) {
        int threshold = 500;
        int value1 = data[i] * 2;
        int value2 = data[i] / 2;
        
        // Modern compilers can optimize this to conditional move instruction
        data[i] = (data[i] >= threshold) ? value1 : value2;
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Conditional move completed in: " << duration.count() << " ms" << endl;
    cout << "Conditional moves avoid branch misprediction penalties" << endl;
}

int main() {
    cout << "Starting optimized branch prediction demonstration..." << endl;
    cout << "Task: Optimizing code for better branch prediction" << endl;
    cout << "Monitor CPU Usage Tool for improved branch prediction performance" << endl << endl;
    
    demonstratePredictableBranching();
    cout << endl;
    demonstrateBranchlessCode();
    cout << endl;
    demonstrateOptimizedBranchLayout();
    cout << endl;
    demonstrateConditionalMove();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- Sorted data creates predictable branch patterns" << endl;
    cout << "- Branchless code eliminates mispredictions entirely" << endl;
    cout << "- Common-case-first layout improves prediction accuracy" << endl;
    cout << "- Conditional moves avoid branch penalties" << endl;
    cout << "- Significantly better instructions per cycle" << endl;
    
    return 0;
}

================================================================================
*/
