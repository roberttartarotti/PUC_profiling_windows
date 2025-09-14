/*
================================================================================
ATIVIDADE PRÁTICA 14 - MEMORY FRAGMENTATION (C++)
================================================================================

OBJETIVO:
- Demonstrar fragmentação de memória com alocações de tamanhos variados
- Usar Memory profiler para identificar padrões de fragmentação
- Otimizar usando memory pools ou allocators customizados
- Medir impacto da fragmentação na performance

PROBLEMA:
- Alocações/dealocações aleatórias fragmentam o heap
- Fragmentação reduz localidade de cache
- Memory profiler mostrará heap fragmentado

SOLUÇÃO:
- Usar memory pools para objetos de tamanho similar
- Alocação em blocos contíguos melhora cache locality

================================================================================
*/

#include <iostream>
#include <chrono>
#include <vector>
#include <random>
#include <algorithm>
using namespace std;

void demonstrateMemoryFragmentation() {
    cout << "Starting memory fragmentation demonstration..." << endl;
    cout << "Monitor Memory profiler - should see fragmented allocation pattern" << endl;
    
    const int ALLOCATIONS = 10000;
    vector<void*> allocatedBlocks;
    
    random_device rd;
    mt19937 gen(rd());
    uniform_int_distribution<> sizeDist(16, 4096); // Random sizes 16B to 4KB
    
    auto start = chrono::high_resolution_clock::now();
    
    // Phase 1: Allocate random sized blocks
    for (int i = 0; i < ALLOCATIONS; i++) {
        int size = sizeDist(gen);
        void* ptr = malloc(size);
        if (ptr) {
            allocatedBlocks.push_back(ptr);
            // Write to memory to ensure it's actually used
            memset(ptr, i % 256, size);
        }
        
        if (i % 1000 == 0) {
            cout << "Allocated " << i << "/" << ALLOCATIONS << " random-sized blocks..." << endl;
        }
    }
    
    // Phase 2: PERFORMANCE ISSUE - Random deallocation creates fragmentation
    shuffle(allocatedBlocks.begin(), allocatedBlocks.end(), gen);
    
    // Free every other block to create holes
    for (size_t i = 0; i < allocatedBlocks.size(); i += 2) {
        free(allocatedBlocks[i]);
        allocatedBlocks[i] = nullptr;
    }
    
    // Phase 3: Try to allocate large contiguous blocks
    vector<void*> largeBlocks;
    const int LARGE_SIZE = 8192; // 8KB blocks
    
    for (int i = 0; i < 100; i++) {
        void* ptr = malloc(LARGE_SIZE);
        if (ptr) {
            largeBlocks.push_back(ptr);
            memset(ptr, 0xFF, LARGE_SIZE);
        } else {
            cout << "Failed to allocate large block " << i << " due to fragmentation!" << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Memory fragmentation test completed in: " << duration.count() << " ms" << endl;
    cout << "Random allocations made: " << ALLOCATIONS << endl;
    cout << "Large blocks successfully allocated: " << largeBlocks.size() << "/100" << endl;
    
    // Cleanup
    for (void* ptr : allocatedBlocks) {
        if (ptr) free(ptr);
    }
    for (void* ptr : largeBlocks) {
        if (ptr) free(ptr);
    }
}

int main() {
    cout << "Starting memory fragmentation demonstration..." << endl;
    cout << "Task: Creating fragmented heap with random allocations" << endl;
    cout << "Monitor Memory Usage Tool for fragmentation patterns" << endl << endl;
    
    demonstrateMemoryFragmentation();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check Memory profiler for:" << endl;
    cout << "- Fragmented heap layout" << endl;
    cout << "- Difficulty allocating large contiguous blocks" << endl;
    cout << "- Reduced memory efficiency" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR MEMORY POOL)
================================================================================

#include <iostream>
#include <chrono>
#include <vector>
#include <array>
using namespace std;

// CORREÇÃO: Simple memory pool for fixed-size allocations
template<size_t BlockSize, size_t BlockCount>
class MemoryPool {
private:
    alignas(max_align_t) char memory[BlockSize * BlockCount];
    bool used[BlockCount] = {false};
    
public:
    void* allocate() {
        for (size_t i = 0; i < BlockCount; i++) {
            if (!used[i]) {
                used[i] = true;
                return &memory[i * BlockSize];
            }
        }
        return nullptr; // Pool exhausted
    }
    
    void deallocate(void* ptr) {
        if (!ptr) return;
        
        char* charPtr = static_cast<char*>(ptr);
        if (charPtr >= memory && charPtr < memory + sizeof(memory)) {
            size_t index = (charPtr - memory) / BlockSize;
            if (index < BlockCount) {
                used[index] = false;
            }
        }
    }
};

void demonstrateMemoryPool() {
    cout << "Starting memory pool demonstration..." << endl;
    cout << "Monitor Memory profiler - should see contiguous allocation pattern" << endl;
    
    const int ALLOCATIONS = 5000;
    
    // CORREÇÃO: Memory pools for different sizes reduce fragmentation
    MemoryPool<64, 2000> smallPool;   // 64-byte blocks
    MemoryPool<256, 2000> mediumPool; // 256-byte blocks
    MemoryPool<1024, 1000> largePool; // 1KB blocks
    
    vector<void*> smallBlocks, mediumBlocks, largeBlocks;
    
    auto start = chrono::high_resolution_clock::now();
    
    // Phase 1: Allocate from pools - no fragmentation
    for (int i = 0; i < ALLOCATIONS / 3; i++) {
        void* small = smallPool.allocate();
        void* medium = mediumPool.allocate();
        void* large = largePool.allocate();
        
        if (small) {
            memset(small, i % 256, 64);
            smallBlocks.push_back(small);
        }
        if (medium) {
            memset(medium, i % 256, 256);
            mediumBlocks.push_back(medium);
        }
        if (large) {
            memset(large, i % 256, 1024);
            largeBlocks.push_back(large);
        }
        
        if (i % 500 == 0) {
            cout << "Pool allocated " << i << " blocks of each size..." << endl;
        }
    }
    
    // Phase 2: Deallocate some blocks - no fragmentation due to pool structure
    for (size_t i = 0; i < smallBlocks.size(); i += 2) {
        smallPool.deallocate(smallBlocks[i]);
    }
    
    // Phase 3: Reallocate - efficient reuse of pool memory
    int reallocated = 0;
    for (int i = 0; i < 500; i++) {
        void* ptr = smallPool.allocate();
        if (ptr) {
            reallocated++;
            memset(ptr, 0xAA, 64);
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Memory pool test completed in: " << duration.count() << " ms" << endl;
    cout << "Pool allocations made: " << ALLOCATIONS << endl;
    cout << "Successful reallocations: " << reallocated << "/500" << endl;
    cout << "No fragmentation due to pool-based allocation" << endl;
}

int main() {
    cout << "Starting optimized memory pool demonstration..." << endl;
    cout << "Task: Using memory pools to prevent fragmentation" << endl;
    cout << "Monitor Memory Usage Tool for contiguous allocation patterns" << endl << endl;
    
    demonstrateMemoryPool();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- No heap fragmentation due to pool allocation" << endl;
    cout << "- Better memory locality and cache performance" << endl;
    cout << "- Predictable allocation/deallocation performance" << endl;
    cout << "- Reduced memory overhead" << endl;
    
    return 0;
}

================================================================================
*/
