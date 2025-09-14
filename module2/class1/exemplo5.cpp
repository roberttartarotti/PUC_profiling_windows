/*
================================================================================
ATIVIDADE PRÁTICA 5 - PARALELIZAÇÃO E USO DE NÚCLEOS DA CPU (C++)
================================================================================

OBJETIVO:
- Implementar processamento sequencial usando loop tradicional
- Refatorar usando paralelismo (std::thread)
- Usar CPU Usage Tool para analisar distribuição do uso de CPU
- Medir ganho de desempenho com utilização de múltiplos núcleos

PROBLEMA:
- Processamento sequencial usa apenas 1 núcleo de CPU (~100% de 1 core)
- Outros núcleos ficam ociosos, desperdiçando capacidade de processamento
- CPU Usage Tool mostrará uso de single-core

SOLUÇÃO:
- Implementar paralelização com std::thread para utilizar todos os núcleos
- Resultado: distribuição de carga across todos os cores disponíveis

================================================================================
*/

#include <iostream>
#include <chrono>
#include <vector>
#include <thread>
#include <mutex>
#include <cmath>
using namespace std;

bool isPrime(long long n) {
    if (n <= 1) return false;
    if (n <= 3) return true;
    if (n % 2 == 0 || n % 3 == 0) return false;
    
    for (long long i = 5; i * i <= n; i += 6) {
        if (n % i == 0 || n % (i + 2) == 0) {
            return false;
        }
    }
    return true;
}

void processExpensiveOperation(long long number, int& primeCount, mutex& mtx) {
    if (isPrime(number)) {
        lock_guard<mutex> lock(mtx);
        primeCount++;
    }
}

void sequentialProcessing(const vector<long long>& numbers) {
    cout << "Starting SEQUENTIAL processing..." << endl;
    cout << "CPU Usage Tool should show single-core utilization" << endl;
    
    auto start = chrono::high_resolution_clock::now();
    int primeCount = 0;
    
    for (size_t i = 0; i < numbers.size(); i++) {
        if (isPrime(numbers[i])) { // CPU INTENSIVE: Single-threaded processing - Use parallel version for multi-core utilization
            primeCount++;
        }
        
        if (i % 1000 == 0) {
            cout << "Sequential progress: " << i << "/" << numbers.size() << " numbers processed" << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "=== SEQUENTIAL RESULTS ===" << endl;
    cout << "Execution time: " << duration.count() << " ms" << endl;
    cout << "Primes found: " << primeCount << endl;
    cout << "CPU cores used: 1 (sequential processing)" << endl << endl;
}

void parallelProcessing(const vector<long long>& numbers) {
    cout << "Starting PARALLEL processing..." << endl;
    cout << "CPU Usage Tool should show multi-core utilization" << endl;
    
    auto start = chrono::high_resolution_clock::now();
    int primeCount = 0;
    mutex mtx;
    
    const size_t numThreads = thread::hardware_concurrency();
    vector<thread> threads;
    size_t chunkSize = numbers.size() / numThreads;
    
    cout << "Using " << numThreads << " threads for parallel processing" << endl;
    
    for (size_t t = 0; t < numThreads; t++) {
        size_t startIdx = t * chunkSize;
        size_t endIdx = (t == numThreads - 1) ? numbers.size() : (t + 1) * chunkSize;
        
        threads.emplace_back([&numbers, startIdx, endIdx, &primeCount, &mtx]() {
            for (size_t i = startIdx; i < endIdx; i++) {
                processExpensiveOperation(numbers[i], primeCount, mtx); // SOLUTION: Multi-threaded processing utilizes all CPU cores
            }
        });
    }
    
    for (auto& t : threads) {
        t.join();
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "=== PARALLEL RESULTS ===" << endl;
    cout << "Execution time: " << duration.count() << " ms" << endl;
    cout << "Primes found: " << primeCount << endl;
    cout << "CPU cores used: " << numThreads << " (parallel processing)" << endl << endl;
}

int main() {
    const size_t DATA_SIZE = 50000;
    cout << "Starting CPU parallelization demonstration..." << endl;
    cout << "Task: Finding prime numbers in range" << endl;
    cout << "Data size: " << DATA_SIZE << " numbers" << endl;
    cout << "Monitor CPU Usage Tool to see single-core vs multi-core utilization" << endl << endl;
    
    vector<long long> numbers;
    numbers.reserve(DATA_SIZE);
    
    for (size_t i = 0; i < DATA_SIZE; i++) {
        numbers.push_back(100000 + i);
    }
    
    cout << "Test data generated (numbers from 100000 to " << (100000 + DATA_SIZE - 1) << ")" << endl << endl;
    
    sequentialProcessing(numbers);
    
    this_thread::sleep_for(chrono::seconds(2));
    
    parallelProcessing(numbers);
    
    cout << "=== PERFORMANCE COMPARISON ===" << endl;
    cout << "Compare the execution times and CPU usage patterns:" << endl;
    cout << "- Sequential: Uses 1 CPU core at ~100%" << endl;
    cout << "- Parallel: Distributes load across all available cores" << endl;
    cout << "- Expected speedup: ~" << thread::hardware_concurrency() << "x (ideal case)" << endl;
    
    return 0;
}

/*
================================================================================
OBSERVAÇÃO: Este exemplo já demonstra ambas as abordagens (sequencial vs paralela)
================================================================================

O código acima já inclui:
1. sequentialProcessing() - demonstra processamento single-threaded
2. parallelProcessing() - demonstra processamento multi-threaded otimizado

Para foco apenas no problema:
- Comente a chamada parallelProcessing() no main()
- Execute apenas sequentialProcessing() para ver uso de single-core

Para foco apenas na solução:
- Comente a chamada sequentialProcessing() no main()  
- Execute apenas parallelProcessing() para ver uso multi-core

================================================================================
*/