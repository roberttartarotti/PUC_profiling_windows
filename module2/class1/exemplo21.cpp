/*
================================================================================
ATIVIDADE PRÁTICA 21 - MEMORY ALIGNMENT PERFORMANCE (C++)
================================================================================

OBJETIVO:
- Demonstrar impacto de memory alignment na performance
- Usar CPU profiler para identificar cache line splits
- Otimizar usando proper struct alignment
- Medir diferença entre aligned vs unaligned data structures

PROBLEMA:
- Unaligned memory access causa cache line splits
- False sharing entre threads
- CPU Profiler mostrará memory access penalties

SOLUÇÃO:
- Proper struct alignment e padding
- Cache line alignment para shared data

================================================================================
*/

#include <iostream>
#include <chrono>
#include <vector>
#include <thread>
#include <atomic>
using namespace std;

// PERFORMANCE ISSUE: Poorly aligned structure
struct UnalignedData {
    char flag;          // 1 byte
    double value;       // 8 bytes - misaligned!
    int counter;        // 4 bytes
    char padding[3];    // Manual padding attempt
};

void demonstrateUnalignedAccess() {
    cout << "Starting unaligned memory access demonstration..." << endl;
    cout << "Monitor CPU profiler - should see memory access penalties" << endl;
    
    const int ITERATIONS = 10000000;
    vector<UnalignedData> data(1000);
    
    // Initialize data
    for (size_t i = 0; i < data.size(); i++) {
        data[i].flag = i % 2;
        data[i].value = i * 3.14159;
        data[i].counter = i;
    }
    
    auto start = chrono::high_resolution_clock::now();
    
    double sum = 0;
    for (int iter = 0; iter < ITERATIONS; iter++) {
        size_t index = iter % data.size();
        // PERFORMANCE ISSUE: Unaligned double access may cause cache line splits
        sum += data[index].value;
        data[index].counter++;
        
        if (iter % 1000000 == 0) {
            cout << "Unaligned access: " << iter << "/" << ITERATIONS << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Unaligned memory access completed in: " << duration.count() << " ms" << endl;
    cout << "Sum: " << sum << endl;
    cout << "Struct size: " << sizeof(UnalignedData) << " bytes" << endl;
}

int main() {
    cout << "Starting memory alignment performance demonstration..." << endl;
    cout << "Task: Accessing potentially unaligned data structures" << endl;
    cout << "Monitor CPU Usage Tool for memory access patterns" << endl << endl;
    
    demonstrateUnalignedAccess();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check CPU profiler for:" << endl;
    cout << "- Memory access penalties" << endl;
    cout << "- Cache line split overhead" << endl;
    cout << "- Unaligned access patterns" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR ALIGNED STRUCTURES)
================================================================================

#include <iostream>
#include <chrono>
#include <vector>
#include <thread>
#include <atomic>
using namespace std;

// CORREÇÃO: Properly aligned structure
struct alignas(64) AlignedData { // Cache line aligned
    double value;       // 8 bytes - aligned to 8-byte boundary
    int counter;        // 4 bytes
    char flag;          // 1 byte
    char padding[51];   // Pad to cache line boundary (64 bytes total)
};

void demonstrateAlignedAccess() {
    cout << "Starting aligned memory access demonstration..." << endl;
    cout << "Monitor CPU profiler - should see improved memory performance" << endl;
    
    const int ITERATIONS = 10000000;
    vector<AlignedData> data(1000);
    
    // Initialize data
    for (size_t i = 0; i < data.size(); i++) {
        data[i].flag = i % 2;
        data[i].value = i * 3.14159;
        data[i].counter = i;
    }
    
    auto start = chrono::high_resolution_clock::now();
    
    double sum = 0;
    for (int iter = 0; iter < ITERATIONS; iter++) {
        size_t index = iter % data.size();
        // CORREÇÃO: Properly aligned access - no cache line splits
        sum += data[index].value;
        data[index].counter++;
        
        if (iter % 1000000 == 0) {
            cout << "Aligned access: " << iter << "/" << ITERATIONS << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Aligned memory access completed in: " << duration.count() << " ms" << endl;
    cout << "Sum: " << sum << endl;
    cout << "Struct size: " << sizeof(AlignedData) << " bytes (cache line aligned)" << endl;
}

// CORREÇÃO: Demonstrating false sharing prevention
struct alignas(64) ThreadData {
    atomic<long long> counter;
    char padding[56]; // Prevent false sharing
};

void demonstrateFalseSharingPrevention() {
    cout << "Starting false sharing prevention demonstration..." << endl;
    
    const int NUM_THREADS = 4;
    const int ITERATIONS = 5000000;
    
    // CORREÇÃO: Each thread gets its own cache line
    vector<ThreadData> threadData(NUM_THREADS);
    
    auto start = chrono::high_resolution_clock::now();
    
    vector<thread> threads;
    for (int t = 0; t < NUM_THREADS; t++) {
        threads.emplace_back([&threadData, t, ITERATIONS]() {
            for (int i = 0; i < ITERATIONS; i++) {
                threadData[t].counter.fetch_add(1, memory_order_relaxed);
            }
        });
    }
    
    for (auto& t : threads) {
        t.join();
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    long long total = 0;
    for (const auto& data : threadData) {
        total += data.counter.load();
    }
    
    cout << "False sharing prevention completed in: " << duration.count() << " ms" << endl;
    cout << "Total count: " << total << endl;
    cout << "Each counter in separate cache line - no false sharing" << endl;
}

int main() {
    cout << "Starting optimized memory alignment demonstration..." << endl;
    cout << "Task: Properly aligned data structures and false sharing prevention" << endl;
    cout << "Monitor CPU Usage Tool for improved memory performance" << endl << endl;
    
    demonstrateAlignedAccess();
    cout << endl;
    demonstrateFalseSharingPrevention();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- Proper alignment eliminates cache line splits" << endl;
    cout << "- Cache line padding prevents false sharing" << endl;
    cout << "- Better memory access patterns" << endl;
    cout << "- Improved performance in multi-threaded scenarios" << endl;
    
    return 0;
}

================================================================================
*/
