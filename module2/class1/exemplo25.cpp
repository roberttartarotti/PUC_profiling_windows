/*
================================================================================
ATIVIDADE PRÁTICA 25 - DYNAMIC MEMORY ALLOCATION OVERHEAD (C++)
================================================================================

OBJETIVO:
- Demonstrar overhead de dynamic memory allocation
- Usar Memory profiler para identificar allocation patterns
- Otimizar usando object pooling e stack allocation
- Medir impacto de frequent new/delete operations

PROBLEMA:
- Frequent new/delete operations são custosas
- Memory fragmentation from variable-sized allocations
- Memory profiler mostrará allocation overhead

SOLUÇÃO:
- Object pooling para reuse
- Stack allocation quando possível
- Custom allocators para specific patterns

================================================================================
*/

#include <iostream>
#include <chrono>
#include <vector>
#include <memory>
#include <random>
using namespace std;

struct DataObject {
    int id;
    double value;
    char buffer[64];
    
    DataObject(int i, double v) : id(i), value(v) {
        for (int j = 0; j < 64; j++) {
            buffer[j] = static_cast<char>(i + j);
        }
    }
};

void demonstrateFrequentAllocation() {
    cout << "Starting frequent allocation demonstration..." << endl;
    cout << "Monitor Memory profiler - should see allocation overhead" << endl;
    
    const int ITERATIONS = 100000;
    vector<unique_ptr<DataObject>> objects;
    objects.reserve(ITERATIONS);
    
    random_device rd;
    mt19937 gen(rd());
    uniform_real_distribution<> dis(0.0, 1000.0);
    
    auto start = chrono::high_resolution_clock::now();
    
    for (int i = 0; i < ITERATIONS; i++) {
        // PERFORMANCE ISSUE: Frequent heap allocation
        auto obj = make_unique<DataObject>(i, dis(gen)); // Heap allocation every iteration
        objects.push_back(move(obj));
        
        // Simulate some work
        objects[i]->value *= 1.1;
        
        if (i % 10000 == 0) {
            cout << "Allocated " << i << "/" << ITERATIONS << " objects..." << endl;
        }
    }
    
    // PERFORMANCE ISSUE: Process objects (causing more allocations)
    vector<unique_ptr<DataObject>> processedObjects;
    for (const auto& obj : objects) {
        if (obj->value > 500.0) {
            // More heap allocation
            processedObjects.push_back(make_unique<DataObject>(obj->id, obj->value * 2));
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Frequent allocation completed in: " << duration.count() << " ms" << endl;
    cout << "Objects created: " << objects.size() << endl;
    cout << "Processed objects: " << processedObjects.size() << endl;
    cout << "Many heap allocations caused overhead" << endl;
}

int main() {
    cout << "Starting dynamic memory allocation demonstration..." << endl;
    cout << "Task: Frequent heap allocation and deallocation" << endl;
    cout << "Monitor Memory profiler for allocation patterns" << endl << endl;
    
    demonstrateFrequentAllocation();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check Memory profiler for:" << endl;
    cout << "- High allocation frequency" << endl;
    cout << "- Memory allocation overhead" << endl;
    cout << "- Heap fragmentation patterns" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR OBJECT POOLING)
================================================================================

#include <iostream>
#include <chrono>
#include <vector>
#include <memory>
#include <random>
#include <stack>
using namespace std;

struct DataObject {
    int id;
    double value;
    char buffer[64];
    
    DataObject() = default;
    
    void initialize(int i, double v) {
        id = i;
        value = v;
        for (int j = 0; j < 64; j++) {
            buffer[j] = static_cast<char>(i + j);
        }
    }
};

// CORREÇÃO: Object pool to reuse allocated objects
class ObjectPool {
private:
    stack<unique_ptr<DataObject>> pool;
    
public:
    unique_ptr<DataObject> acquire() {
        if (pool.empty()) {
            return make_unique<DataObject>(); // Only allocate if pool is empty
        } else {
            auto obj = move(pool.top());
            pool.pop();
            return obj;
        }
    }
    
    void release(unique_ptr<DataObject> obj) {
        pool.push(move(obj)); // Return to pool instead of deallocating
    }
    
    size_t size() const { return pool.size(); }
};

void demonstrateObjectPooling() {
    cout << "Starting object pooling demonstration..." << endl;
    cout << "Monitor Memory profiler - should see reduced allocation overhead" << endl;
    
    const int ITERATIONS = 100000;
    ObjectPool pool;
    vector<unique_ptr<DataObject>> objects;
    objects.reserve(ITERATIONS);
    
    random_device rd;
    mt19937 gen(rd());
    uniform_real_distribution<> dis(0.0, 1000.0);
    
    auto start = chrono::high_resolution_clock::now();
    
    for (int i = 0; i < ITERATIONS; i++) {
        // CORREÇÃO: Acquire from pool - reuses memory
        auto obj = pool.acquire();
        obj->initialize(i, dis(gen));
        objects.push_back(move(obj));
        
        objects[i]->value *= 1.1;
        
        if (i % 10000 == 0) {
            cout << "Pool allocated " << i << "/" << ITERATIONS << " objects..." << endl;
        }
    }
    
    // CORREÇÃO: Return objects to pool for reuse
    vector<unique_ptr<DataObject>> processedObjects;
    for (auto& obj : objects) {
        if (obj->value > 500.0) {
            auto processed = pool.acquire(); // Reuse from pool
            processed->initialize(obj->id, obj->value * 2);
            processedObjects.push_back(move(processed));
        }
        pool.release(move(obj)); // Return to pool
    }
    objects.clear();
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Object pooling completed in: " << duration.count() << " ms" << endl;
    cout << "Pool size after processing: " << pool.size() << endl;
    cout << "Processed objects: " << processedObjects.size() << endl;
    cout << "Object pooling significantly reduced allocations" << endl;
    
    // Return processed objects to pool
    for (auto& obj : processedObjects) {
        pool.release(move(obj));
    }
    
    cout << "Final pool size: " << pool.size() << endl;
}

void demonstrateStackAllocation() {
    cout << "Starting stack allocation demonstration..." << endl;
    
    const int ITERATIONS = 1000000;
    
    auto start = chrono::high_resolution_clock::now();
    
    double totalValue = 0;
    
    for (int i = 0; i < ITERATIONS; i++) {
        // CORREÇÃO: Stack allocation - no heap overhead
        DataObject obj; // Stack allocated - very fast
        obj.initialize(i, i * 0.1);
        
        totalValue += obj.value;
        
        if (i % 100000 == 0) {
            cout << "Stack allocated " << i << "/" << ITERATIONS << " objects..." << endl;
        }
    } // Automatic cleanup when leaving scope
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Stack allocation completed in: " << duration.count() << " ms" << endl;
    cout << "Total value: " << totalValue << endl;
    cout << "Stack allocation is fastest - no malloc/free overhead" << endl;
}

// CORREÇÃO: Custom allocator for specific allocation patterns
class FixedSizeAllocator {
private:
    vector<char> memory;
    vector<void*> freeBlocks;
    size_t blockSize;
    size_t blockCount;
    
public:
    FixedSizeAllocator(size_t size, size_t count) 
        : blockSize(size), blockCount(count) {
        memory.resize(blockSize * blockCount);
        
        // Initialize free blocks
        for (size_t i = 0; i < blockCount; i++) {
            freeBlocks.push_back(&memory[i * blockSize]);
        }
    }
    
    void* allocate() {
        if (freeBlocks.empty()) {
            return nullptr; // Allocator exhausted
        }
        
        void* block = freeBlocks.back();
        freeBlocks.pop_back();
        return block;
    }
    
    void deallocate(void* ptr) {
        if (ptr >= &memory[0] && ptr < &memory[memory.size()]) {
            freeBlocks.push_back(ptr);
        }
    }
    
    size_t available() const { return freeBlocks.size(); }
};

void demonstrateCustomAllocator() {
    cout << "Starting custom allocator demonstration..." << endl;
    
    const int ITERATIONS = 50000;
    FixedSizeAllocator allocator(sizeof(DataObject), ITERATIONS);
    vector<DataObject*> objects;
    objects.reserve(ITERATIONS);
    
    auto start = chrono::high_resolution_clock::now();
    
    for (int i = 0; i < ITERATIONS; i++) {
        // CORREÇÃO: Custom allocator - predictable performance
        void* memory = allocator.allocate();
        if (memory) {
            DataObject* obj = new(memory) DataObject(); // Placement new
            obj->initialize(i, i * 1.5);
            objects.push_back(obj);
        }
        
        if (i % 10000 == 0) {
            cout << "Custom allocated " << i << "/" << ITERATIONS 
                 << ", available: " << allocator.available() << endl;
        }
    }
    
    // Clean up
    for (auto obj : objects) {
        obj->~DataObject(); // Explicit destructor call
        allocator.deallocate(obj);
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Custom allocator completed in: " << duration.count() << " ms" << endl;
    cout << "Final available blocks: " << allocator.available() << endl;
    cout << "Custom allocator provided predictable performance" << endl;
}

int main() {
    cout << "Starting optimized memory allocation demonstration..." << endl;
    cout << "Task: Reducing allocation overhead with various strategies" << endl;
    cout << "Monitor Memory profiler for improved allocation patterns" << endl << endl;
    
    demonstrateObjectPooling();
    cout << endl;
    demonstrateStackAllocation();
    cout << endl;
    demonstrateCustomAllocator();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Performance ranking (fastest to slowest):" << endl;
    cout << "1. Stack allocation - no malloc/free overhead" << endl;
    cout << "2. Custom allocator - predictable fixed-size allocation" << endl;
    cout << "3. Object pooling - reuses heap memory" << endl;
    cout << "4. Standard heap allocation - highest overhead" << endl;
    cout << endl;
    cout << "Improvements:" << endl;
    cout << "- Object pooling eliminates repeated allocation/deallocation" << endl;
    cout << "- Stack allocation is fastest for temporary objects" << endl;
    cout << "- Custom allocators provide predictable performance" << endl;
    cout << "- Dramatically reduced memory allocation overhead" << endl;
    
    return 0;
}

================================================================================
*/
