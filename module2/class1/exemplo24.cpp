/*
================================================================================
ATIVIDADE PRÁTICA 24 - FALSE SHARING PERFORMANCE (C++)
================================================================================

OBJETIVO:
- Demonstrar false sharing entre threads
- Usar CPU profiler para identificar cache coherency overhead
- Otimizar usando cache line padding
- Medir impacto de false sharing na scalabilidade

PROBLEMA:
- Multiple threads accessing nearby memory locations
- Cache line invalidation entre cores
- CPU Profiler mostrará poor scaling com more threads

SOLUÇÃO:
- Cache line alignment e padding
- Separate data accessed by different threads

================================================================================
*/

#include <iostream>
#include <chrono>
#include <thread>
#include <vector>
#include <atomic>
using namespace std;

// PERFORMANCE ISSUE: Data structure that causes false sharing
struct BadCounters {
    atomic<long long> counter1;  // Same cache line
    atomic<long long> counter2;  // Same cache line - FALSE SHARING!
    atomic<long long> counter3;  // Same cache line
    atomic<long long> counter4;  // Same cache line
};

void demonstrateFalseSharing() {
    cout << "Starting false sharing demonstration..." << endl;
    cout << "Monitor CPU profiler - should see poor thread scaling" << endl;
    
    const int NUM_THREADS = 4;
    const int ITERATIONS = 5000000;
    
    BadCounters counters = {{0}, {0}, {0}, {0}};
    vector<thread> threads;
    
    auto start = chrono::high_resolution_clock::now();
    
    // PERFORMANCE ISSUE: Each thread modifies different counter, but same cache line
    for (int t = 0; t < NUM_THREADS; t++) {
        threads.emplace_back([&counters, t, ITERATIONS]() {
            for (int i = 0; i < ITERATIONS; i++) {
                // Each thread accesses a different counter, but they're in same cache line
                switch (t) {
                    case 0: counters.counter1.fetch_add(1, memory_order_relaxed); break;
                    case 1: counters.counter2.fetch_add(1, memory_order_relaxed); break;
                    case 2: counters.counter3.fetch_add(1, memory_order_relaxed); break;
                    case 3: counters.counter4.fetch_add(1, memory_order_relaxed); break;
                }
                
                if (i % 1000000 == 0) {
                    cout << "Thread " << t << ": " << i << "/" << ITERATIONS << endl;
                }
            }
        });
    }
    
    for (auto& t : threads) {
        t.join();
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "False sharing test completed in: " << duration.count() << " ms" << endl;
    cout << "Counter values: " << counters.counter1 << ", " << counters.counter2 
         << ", " << counters.counter3 << ", " << counters.counter4 << endl;
    cout << "Poor performance due to false sharing between threads" << endl;
}

int main() {
    cout << "Starting false sharing performance demonstration..." << endl;
    cout << "Task: Multiple threads accessing data in same cache lines" << endl;
    cout << "Monitor CPU Usage Tool for cache coherency overhead" << endl << endl;
    
    demonstrateFalseSharing();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check CPU profiler for:" << endl;
    cout << "- Poor scaling with multiple threads" << endl;
    cout << "- Cache coherency overhead" << endl;
    cout << "- High cache miss rates" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA ELIMINAR FALSE SHARING)
================================================================================

#include <iostream>
#include <chrono>
#include <thread>
#include <vector>
#include <atomic>
using namespace std;

// CORREÇÃO: Cache line aligned structure to prevent false sharing
struct alignas(64) AlignedCounter {
    atomic<long long> counter;
    char padding[64 - sizeof(atomic<long long>)]; // Pad to cache line size
};

struct OptimizedCounters {
    AlignedCounter counter1;  // Each counter in its own cache line
    AlignedCounter counter2;  // No false sharing
    AlignedCounter counter3;
    AlignedCounter counter4;
};

void demonstrateOptimizedSharing() {
    cout << "Starting optimized sharing demonstration..." << endl;
    cout << "Monitor CPU profiler - should see better thread scaling" << endl;
    
    const int NUM_THREADS = 4;
    const int ITERATIONS = 5000000;
    
    OptimizedCounters counters = {{{0}}, {{0}}, {{0}}, {{0}}};
    vector<thread> threads;
    
    auto start = chrono::high_resolution_clock::now();
    
    // CORREÇÃO: Each thread accesses its own cache line - no false sharing
    for (int t = 0; t < NUM_THREADS; t++) {
        threads.emplace_back([&counters, t, ITERATIONS]() {
            for (int i = 0; i < ITERATIONS; i++) {
                // Each counter is in its own cache line now
                switch (t) {
                    case 0: counters.counter1.counter.fetch_add(1, memory_order_relaxed); break;
                    case 1: counters.counter2.counter.fetch_add(1, memory_order_relaxed); break;
                    case 2: counters.counter3.counter.fetch_add(1, memory_order_relaxed); break;
                    case 3: counters.counter4.counter.fetch_add(1, memory_order_relaxed); break;
                }
                
                if (i % 1000000 == 0) {
                    cout << "Optimized thread " << t << ": " << i << "/" << ITERATIONS << endl;
                }
            }
        });
    }
    
    for (auto& t : threads) {
        t.join();
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Optimized sharing test completed in: " << duration.count() << " ms" << endl;
    cout << "Counter values: " << counters.counter1.counter << ", " << counters.counter2.counter 
         << ", " << counters.counter3.counter << ", " << counters.counter4.counter << endl;
    cout << "Better performance - no false sharing" << endl;
}

void demonstrateThreadLocalStorage() {
    cout << "Starting thread local storage demonstration..." << endl;
    
    const int NUM_THREADS = 4;
    const int ITERATIONS = 5000000;
    
    // CORREÇÃO: Use thread-local storage to avoid sharing entirely
    thread_local long long local_counter = 0;
    vector<thread> threads;
    vector<long long> results(NUM_THREADS);
    
    auto start = chrono::high_resolution_clock::now();
    
    for (int t = 0; t < NUM_THREADS; t++) {
        threads.emplace_back([&results, t, ITERATIONS]() {
            local_counter = 0; // Thread-local variable
            
            for (int i = 0; i < ITERATIONS; i++) {
                local_counter++; // No atomic operations needed
            }
            
            results[t] = local_counter; // Store result
        });
    }
    
    for (auto& t : threads) {
        t.join();
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    long long total = 0;
    for (auto result : results) {
        total += result;
    }
    
    cout << "Thread local storage completed in: " << duration.count() << " ms" << endl;
    cout << "Total count: " << total << endl;
    cout << "Thread-local storage eliminates all sharing" << endl;
}

void demonstrateWorkPartitioning() {
    cout << "Starting work partitioning demonstration..." << endl;
    
    const int NUM_THREADS = 4;
    const int TOTAL_WORK = 20000000;
    const int WORK_PER_THREAD = TOTAL_WORK / NUM_THREADS;
    
    vector<long long> results(NUM_THREADS);
    vector<thread> threads;
    
    auto start = chrono::high_resolution_clock::now();
    
    // CORREÇÃO: Partition work so threads don't share data
    for (int t = 0; t < NUM_THREADS; t++) {
        threads.emplace_back([&results, t, WORK_PER_THREAD]() {
            long long local_sum = 0;
            int start_idx = t * WORK_PER_THREAD;
            int end_idx = start_idx + WORK_PER_THREAD;
            
            for (int i = start_idx; i < end_idx; i++) {
                local_sum += i; // Work on completely separate ranges
            }
            
            results[t] = local_sum; // Write to separate array elements
        });
    }
    
    for (auto& t : threads) {
        t.join();
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    long long total = 0;
    for (auto result : results) {
        total += result;
    }
    
    cout << "Work partitioning completed in: " << duration.count() << " ms" << endl;
    cout << "Total sum: " << total << endl;
    cout << "Work partitioning avoids data sharing entirely" << endl;
}

int main() {
    cout << "Starting optimized sharing demonstration..." << endl;
    cout << "Task: Eliminating false sharing between threads" << endl;
    cout << "Monitor CPU Usage Tool for improved thread scaling" << endl << endl;
    
    demonstrateOptimizedSharing();
    cout << endl;
    demonstrateThreadLocalStorage();
    cout << endl;
    demonstrateWorkPartitioning();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- Cache line alignment eliminates false sharing" << endl;
    cout << "- Thread-local storage avoids sharing entirely" << endl;
    cout << "- Work partitioning prevents data contention" << endl;
    cout << "- Much better scaling with multiple threads" << endl;
    cout << "- Reduced cache coherency overhead" << endl;
    
    return 0;
}

================================================================================
*/
