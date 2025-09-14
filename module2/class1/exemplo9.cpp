/*
================================================================================
ATIVIDADE PRÁTICA 9 - OVERHEAD DE VIRTUAL FUNCTIONS (C++)
================================================================================

OBJETIVO:
- Demonstrar overhead de virtual function calls em loops intensivos
- Usar CPU profiler para identificar custos de virtual dispatch
- Otimizar usando templates ou função direta
- Comparar performance de virtual vs non-virtual calls

PROBLEMA:
- Virtual function calls requerem vtable lookup
- Indirect jumps impedem otimizações do compilador
- CPU Profiler mostrará tempo gasto em function call overhead

SOLUÇÃO:
- Usar templates para static dispatch
- Considerar direct function calls quando polimorfismo não é necessário

================================================================================
*/

#include <iostream>
#include <chrono>
#include <vector>
#include <memory>
using namespace std;

class VirtualProcessor {
public:
    virtual ~VirtualProcessor() = default;
    
    // PERFORMANCE ISSUE: Virtual function called in tight loops
    virtual int processValue(int value) = 0;
};

class ConcreteProcessor : public VirtualProcessor {
public:
    // Virtual function with vtable lookup overhead
    int processValue(int value) override {
        return value * value + 1; // Simple operation with virtual call overhead
    }
};

class DirectProcessor {
public:
    // Non-virtual function - direct call, compiler can inline
    int processValue(int value) {
        return value * value + 1; // Same operation without virtual overhead
    }
};

void demonstrateVirtualOverhead() {
    cout << "Starting virtual function overhead demonstration..." << endl;
    cout << "Monitor CPU profiler - should see virtual dispatch overhead" << endl;
    
    const int ITERATIONS = 10000000;
    vector<unique_ptr<VirtualProcessor>> processors;
    
    // Create processors
    for (int i = 0; i < 10; i++) {
        processors.push_back(make_unique<ConcreteProcessor>());
    }
    
    auto start = chrono::high_resolution_clock::now();
    
    long long totalResult = 0;
    for (int i = 0; i < ITERATIONS; i++) {
        // PERFORMANCE BOTTLENECK: Virtual function call in tight loop
        int result = processors[i % processors.size()]->processValue(i);
        totalResult += result;
        
        if (i % 1000000 == 0) {
            cout << "Processed " << i << "/" << ITERATIONS << " virtual calls..." << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Virtual function processing completed in: " << duration.count() << " ms" << endl;
    cout << "Total result: " << totalResult << endl;
    cout << "Virtual calls made: " << ITERATIONS << endl;
}

int main() {
    cout << "Starting virtual function performance demonstration..." << endl;
    cout << "Task: Processing values using virtual function calls" << endl;
    cout << "Monitor CPU Usage Tool for virtual dispatch overhead" << endl << endl;
    
    demonstrateVirtualOverhead();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check CPU profiler for:" << endl;
    cout << "- Time spent in virtual function dispatch" << endl;
    cout << "- vtable lookup overhead" << endl;
    cout << "- Reduced compiler optimization opportunities" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR A VERSÃO OTIMIZADA)
================================================================================

#include <iostream>
#include <chrono>
#include <vector>
using namespace std;

class DirectProcessor {
public:
    // CORREÇÃO: Non-virtual function allows compiler optimization
    inline int processValue(int value) {
        return value * value + 1; // Can be inlined by compiler
    }
};

template<typename ProcessorType>
void demonstrateDirectCalls() {
    cout << "Starting direct function call demonstration..." << endl;
    cout << "Monitor CPU profiler - should see reduced call overhead" << endl;
    
    const int ITERATIONS = 10000000;
    vector<ProcessorType> processors(10);
    
    auto start = chrono::high_resolution_clock::now();
    
    long long totalResult = 0;
    for (int i = 0; i < ITERATIONS; i++) {
        // CORREÇÃO: Direct function call - can be inlined, no vtable lookup
        int result = processors[i % processors.size()].processValue(i);
        totalResult += result;
        
        if (i % 1000000 == 0) {
            cout << "Processed " << i << "/" << ITERATIONS << " direct calls..." << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Direct function processing completed in: " << duration.count() << " ms" << endl;
    cout << "Total result: " << totalResult << endl;
    cout << "Function calls made: " << ITERATIONS << " (optimized)" << endl;
}

int main() {
    cout << "Starting optimized function call demonstration..." << endl;
    cout << "Task: Processing values using direct/template function calls" << endl;
    cout << "Monitor CPU Usage Tool for improved performance" << endl << endl;
    
    demonstrateDirectCalls<DirectProcessor>();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- No virtual function dispatch overhead" << endl;
    cout << "- Function inlining opportunities" << endl;
    cout << "- Better compiler optimizations" << endl;
    cout << "- Predictable branch patterns" << endl;
    
    return 0;
}

================================================================================
*/
