/*
================================================================================
ATIVIDADE PRÁTICA 19 - NETWORK I/O BLOCKING PERFORMANCE (C++)
================================================================================

OBJETIVO:
- Demonstrar impacto de blocking network I/O na performance
- Usar I/O profiler para identificar network wait times
- Otimizar usando async I/O e connection pooling
- Medir latência vs throughput em network operations

PROBLEMA:
- Blocking socket operations param threads
- Sequential network requests são ineficientes
- I/O Profiler mostrará threads blocked em network waits

SOLUÇÃO:
- Async I/O para concurrent network operations
- Connection reuse e pooling

================================================================================
*/

#include <iostream>
#include <chrono>
#include <thread>
#include <vector>
#include <string>
using namespace std;

void simulateNetworkRequest(const string& url, int requestId) {
    // Simulate network latency (blocking operation)
    int latencyMs = 100 + (requestId % 200); // 100-300ms latency
    
    cout << "Thread " << this_thread::get_id() 
         << " starting request " << requestId << " to " << url << endl;
    
    // PERFORMANCE ISSUE: Blocking sleep simulates network I/O
    this_thread::sleep_for(chrono::milliseconds(latencyMs));
    
    cout << "Request " << requestId << " completed in " << latencyMs << "ms" << endl;
}

void demonstrateBlockingNetworkIO() {
    cout << "Starting blocking network I/O demonstration..." << endl;
    cout << "Monitor I/O profiler - should see sequential blocking requests" << endl;
    
    const int NUM_REQUESTS = 10;
    vector<string> urls = {
        "http://api1.example.com/data",
        "http://api2.example.com/users", 
        "http://api3.example.com/orders",
        "http://api4.example.com/products"
    };
    
    auto start = chrono::high_resolution_clock::now();
    
    // PERFORMANCE ISSUE: Sequential blocking network requests
    for (int i = 0; i < NUM_REQUESTS; i++) {
        string url = urls[i % urls.size()];
        simulateNetworkRequest(url, i); // Blocks until completion
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Blocking network I/O completed in: " << duration.count() << " ms" << endl;
    cout << "Requests processed: " << NUM_REQUESTS << endl;
    cout << "Average time per request: " << (duration.count() / NUM_REQUESTS) << " ms" << endl;
    cout << "All requests were processed sequentially (inefficient)" << endl;
}

int main() {
    cout << "Starting network I/O performance demonstration..." << endl;
    cout << "Task: Making sequential network requests" << endl;
    cout << "Monitor I/O profiling tools for network wait times" << endl << endl;
    
    demonstrateBlockingNetworkIO();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check I/O profiler for:" << endl;
    cout << "- Long network wait times" << endl;
    cout << "- Sequential request processing" << endl;
    cout << "- Poor network throughput utilization" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR CONCURRENT NETWORK I/O)
================================================================================

#include <iostream>
#include <chrono>
#include <thread>
#include <vector>
#include <string>
#include <future>
using namespace std;

future<int> asyncNetworkRequest(const string& url, int requestId) {
    return async(launch::async, [url, requestId]() {
        // Simulate network latency
        int latencyMs = 100 + (requestId % 200); // 100-300ms latency
        
        cout << "Async request " << requestId << " started to " << url << endl;
        
        // CORREÇÃO: This runs in separate thread, not blocking main thread
        this_thread::sleep_for(chrono::milliseconds(latencyMs));
        
        cout << "Async request " << requestId << " completed in " << latencyMs << "ms" << endl;
        return latencyMs;
    });
}

void demonstrateConcurrentNetworkIO() {
    cout << "Starting concurrent network I/O demonstration..." << endl;
    cout << "Monitor I/O profiler - should see concurrent request processing" << endl;
    
    const int NUM_REQUESTS = 10;
    vector<string> urls = {
        "http://api1.example.com/data",
        "http://api2.example.com/users", 
        "http://api3.example.com/orders",
        "http://api4.example.com/products"
    };
    
    auto start = chrono::high_resolution_clock::now();
    
    // CORREÇÃO: Launch all requests concurrently
    vector<future<int>> futures;
    
    for (int i = 0; i < NUM_REQUESTS; i++) {
        string url = urls[i % urls.size()];
        futures.push_back(asyncNetworkRequest(url, i));
    }
    
    // CORREÇÃO: Wait for all requests to complete
    int totalLatency = 0;
    for (auto& future : futures) {
        totalLatency += future.get(); // Non-blocking since requests run concurrently
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Concurrent network I/O completed in: " << duration.count() << " ms" << endl;
    cout << "Requests processed: " << NUM_REQUESTS << endl;
    cout << "Total latency (if sequential): " << totalLatency << " ms" << endl;
    cout << "Speedup from concurrency: " << (totalLatency / duration.count()) << "x" << endl;
}

class ConnectionPool {
private:
    vector<int> availableConnections;
    mutex poolMutex;
    
public:
    ConnectionPool(int size) {
        for (int i = 0; i < size; i++) {
            availableConnections.push_back(i); // Simulate connection IDs
        }
    }
    
    int acquireConnection() {
        lock_guard<mutex> lock(poolMutex);
        if (!availableConnections.empty()) {
            int conn = availableConnections.back();
            availableConnections.pop_back();
            return conn;
        }
        return -1; // No available connections
    }
    
    void releaseConnection(int connectionId) {
        lock_guard<mutex> lock(poolMutex);
        availableConnections.push_back(connectionId);
    }
    
    size_t availableCount() {
        lock_guard<mutex> lock(poolMutex);
        return availableConnections.size();
    }
};

void demonstrateConnectionPooling() {
    cout << "Starting connection pooling demonstration..." << endl;
    
    const int NUM_REQUESTS = 20;
    const int POOL_SIZE = 5;
    
    ConnectionPool pool(POOL_SIZE);
    vector<future<void>> futures;
    
    auto start = chrono::high_resolution_clock::now();
    
    // CORREÇÃO: Use connection pool for better resource management
    for (int i = 0; i < NUM_REQUESTS; i++) {
        futures.push_back(async(launch::async, [&pool, i]() {
            // Acquire connection from pool
            int conn = pool.acquireConnection();
            while (conn == -1) {
                this_thread::sleep_for(chrono::milliseconds(10));
                conn = pool.acquireConnection();
            }
            
            cout << "Request " << i << " using connection " << conn << endl;
            
            // Simulate network work
            this_thread::sleep_for(chrono::milliseconds(50 + (i % 100)));
            
            // Release connection back to pool
            pool.releaseConnection(conn);
            cout << "Request " << i << " released connection " << conn << endl;
        }));
    }
    
    // Wait for all requests
    for (auto& future : futures) {
        future.get();
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Connection pooling completed in: " << duration.count() << " ms" << endl;
    cout << "Final available connections: " << pool.availableCount() << "/" << POOL_SIZE << endl;
}

int main() {
    cout << "Starting optimized network I/O demonstration..." << endl;
    cout << "Task: Concurrent network requests with connection pooling" << endl;
    cout << "Monitor I/O profiling tools for improved throughput" << endl << endl;
    
    demonstrateConcurrentNetworkIO();
    cout << endl;
    demonstrateConnectionPooling();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- Concurrent requests dramatically reduce total time" << endl;
    cout << "- Connection pooling prevents resource exhaustion" << endl;
    cout << "- Much better network bandwidth utilization" << endl;
    cout << "- Scalable to handle many more concurrent requests" << endl;
    
    return 0;
}

================================================================================
*/
