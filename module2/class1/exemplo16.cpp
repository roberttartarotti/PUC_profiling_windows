/*
================================================================================
ATIVIDADE PRÁTICA 16 - LOCK CONTENTION PERFORMANCE (C++)
================================================================================

OBJETIVO:
- Demonstrar degradação de performance devido à contention de locks
- Usar CPU profiler para identificar threads bloqueadas aguardando locks
- Otimizar usando fine-grained locking ou lock-free structures
- Medir impacto de lock contention na escalabilidade

PROBLEMA:
- Single global mutex causa serialização de threads
- Lock contention reduz paralelismo efetivo
- CPU Profiler mostrará threads blocked em mutex wait

SOLUÇÃO:
- Fine-grained locking ou partitioning
- Lock-free data structures quando possível

================================================================================
*/

#include <iostream>
#include <thread>
#include <mutex>
#include <vector>
#include <chrono>
#include <random>
using namespace std;

class CoarseGrainedCounter {
private:
    mutex globalMutex;
    vector<int> counters;
    
public:
    CoarseGrainedCounter(int size) : counters(size, 0) {}
    
    void increment(int index) {
        // PERFORMANCE ISSUE: Single global lock for all operations
        lock_guard<mutex> lock(globalMutex); // All threads contend for same lock
        
        counters[index]++;
        
        // Simulate some work while holding the lock
        this_thread::sleep_for(chrono::microseconds(1));
    }
    
    int getSum() {
        lock_guard<mutex> lock(globalMutex);
        int sum = 0;
        for (int val : counters) {
            sum += val;
        }
        return sum;
    }
};

void demonstrateLockContention() {
    cout << "Starting lock contention demonstration..." << endl;
    cout << "Monitor CPU profiler - should see threads blocked on mutex wait" << endl;
    
    const int NUM_THREADS = 8;
    const int OPERATIONS_PER_THREAD = 1000;
    const int COUNTER_SIZE = 100;
    
    CoarseGrainedCounter counter(COUNTER_SIZE);
    vector<thread> threads;
    
    random_device rd;
    mt19937 gen(rd());
    uniform_int_distribution<> indexDist(0, COUNTER_SIZE - 1);
    
    auto start = chrono::high_resolution_clock::now();
    
    // Launch threads that will contend for the same global lock
    for (int t = 0; t < NUM_THREADS; t++) {
        threads.emplace_back([&counter, &indexDist, OPERATIONS_PER_THREAD, t]() {
            mt19937 localGen(t); // Thread-local generator
            uniform_int_distribution<> localDist(0, 99);
            
            for (int i = 0; i < OPERATIONS_PER_THREAD; i++) {
                int index = localDist(localGen);
                counter.increment(index); // All threads compete for global lock
                
                if (i % 200 == 0) {
                    cout << "Thread " << t << " completed " << i << "/" << OPERATIONS_PER_THREAD << " operations" << endl;
                }
            }
        });
    }
    
    // Wait for all threads
    for (auto& t : threads) {
        t.join();
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Lock contention test completed in: " << duration.count() << " ms" << endl;
    cout << "Total sum: " << counter.getSum() << endl;
    cout << "Threads: " << NUM_THREADS << ", Operations per thread: " << OPERATIONS_PER_THREAD << endl;
    cout << "Lock contention severely limited parallelism" << endl;
}

int main() {
    cout << "Starting lock contention demonstration..." << endl;
    cout << "Task: Multiple threads incrementing counters with global lock" << endl;
    cout << "Monitor CPU Usage Tool for lock contention and thread blocking" << endl << endl;
    
    demonstrateLockContention();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check CPU profiler for:" << endl;
    cout << "- Threads blocked waiting for mutex" << endl;
    cout << "- Low CPU utilization due to serialization" << endl;
    cout << "- Lock contention overhead" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR FINE-GRAINED LOCKING)
================================================================================

#include <iostream>
#include <thread>
#include <mutex>
#include <vector>
#include <chrono>
#include <random>
using namespace std;

class FineGrainedCounter {
private:
    vector<mutex> mutexes;  // One mutex per counter
    vector<int> counters;
    
public:
    FineGrainedCounter(int size) : mutexes(size), counters(size, 0) {}
    
    void increment(int index) {
        // CORREÇÃO: Lock only the specific counter, not everything
        lock_guard<mutex> lock(mutexes[index]); // Fine-grained locking
        
        counters[index]++;
        
        // Same work, but now only blocks access to this specific counter
        this_thread::sleep_for(chrono::microseconds(1));
    }
    
    int getSum() {
        // CORREÇÃO: Lock all mutexes in deterministic order to avoid deadlock
        vector<unique_lock<mutex>> locks;
        for (auto& m : mutexes) {
            locks.emplace_back(m);
        }
        
        int sum = 0;
        for (int val : counters) {
            sum += val;
        }
        return sum;
    }
};

// Alternative: Lock-free approach using atomic operations
class LockFreeCounter {
private:
    vector<atomic<int>> counters;
    
public:
    LockFreeCounter(int size) : counters(size) {
        for (auto& counter : counters) {
            counter = 0;
        }
    }
    
    void increment(int index) {
        // CORREÇÃO: Atomic increment - no locks needed
        counters[index].fetch_add(1, memory_order_relaxed);
        
        // Simulate work without holding any locks
        this_thread::sleep_for(chrono::microseconds(1));
    }
    
    int getSum() {
        int sum = 0;
        for (const auto& counter : counters) {
            sum += counter.load(memory_order_relaxed);
        }
        return sum;
    }
};

void demonstrateFineGrainedLocking() {
    cout << "Starting fine-grained locking demonstration..." << endl;
    cout << "Monitor CPU profiler - should see improved parallelism" << endl;
    
    const int NUM_THREADS = 8;
    const int OPERATIONS_PER_THREAD = 1000;
    const int COUNTER_SIZE = 100;
    
    FineGrainedCounter counter(COUNTER_SIZE);
    vector<thread> threads;
    
    auto start = chrono::high_resolution_clock::now();
    
    // Launch threads - now they can work in parallel on different counters
    for (int t = 0; t < NUM_THREADS; t++) {
        threads.emplace_back([&counter, OPERATIONS_PER_THREAD, COUNTER_SIZE, t]() {
            mt19937 localGen(t);
            uniform_int_distribution<> localDist(0, COUNTER_SIZE - 1);
            
            for (int i = 0; i < OPERATIONS_PER_THREAD; i++) {
                int index = localDist(localGen);
                counter.increment(index); // Much less contention
            }
        });
    }
    
    for (auto& t : threads) {
        t.join();
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Fine-grained locking completed in: " << duration.count() << " ms" << endl;
    cout << "Total sum: " << counter.getSum() << endl;
}

void demonstrateLockFree() {
    cout << "Starting lock-free demonstration..." << endl;
    
    const int NUM_THREADS = 8;
    const int OPERATIONS_PER_THREAD = 1000;
    const int COUNTER_SIZE = 100;
    
    LockFreeCounter counter(COUNTER_SIZE);
    vector<thread> threads;
    
    auto start = chrono::high_resolution_clock::now();
    
    // Launch threads - completely lock-free operation
    for (int t = 0; t < NUM_THREADS; t++) {
        threads.emplace_back([&counter, OPERATIONS_PER_THREAD, COUNTER_SIZE, t]() {
            mt19937 localGen(t);
            uniform_int_distribution<> localDist(0, COUNTER_SIZE - 1);
            
            for (int i = 0; i < OPERATIONS_PER_THREAD; i++) {
                int index = localDist(localGen);
                counter.increment(index); // No locks at all!
            }
        });
    }
    
    for (auto& t : threads) {
        t.join();
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Lock-free approach completed in: " << duration.count() << " ms" << endl;
    cout << "Total sum: " << counter.getSum() << endl;
}

int main() {
    cout << "Starting optimized locking demonstration..." << endl;
    cout << "Task: Comparing different locking strategies" << endl;
    cout << "Monitor CPU Usage Tool for improved parallelism" << endl << endl;
    
    demonstrateFineGrainedLocking();
    cout << endl;
    demonstrateLockFree();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- Fine-grained locking reduces contention" << endl;
    cout << "- Lock-free atomics eliminate blocking entirely" << endl;
    cout << "- Much better thread utilization and scalability" << endl;
    cout << "- Significantly faster execution" << endl;
    
    return 0;
}

================================================================================
*/
