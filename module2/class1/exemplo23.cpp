/*
================================================================================
ATIVIDADE PRÁTICA 23 - TEMPLATE INSTANTIATION OVERHEAD (C++)
================================================================================

OBJETIVO:
- Demonstrar overhead de template instantiation excessive
- Usar compilation profiler para identificar compile-time bottlenecks
- Otimizar usando explicit instantiation e type erasure
- Medir compile time impact de template abuse

PROBLEMA:
- Excessive template instantiations aumentam compile time
- Code bloat from duplicate template instantiations
- Compilation profiler mostrará template expansion overhead

SOLUÇÃO:
- Explicit template instantiation
- Type erasure para reduce template instantiations
- Move implementation para .cpp files

================================================================================
*/

#include <iostream>
#include <chrono>
#include <vector>
#include <type_traits>
using namespace std;

// PERFORMANCE ISSUE: Heavy template that gets instantiated many times
template<typename T, int N, bool UseCache = true, bool UseLogging = false>
class ExpensiveTemplate {
private:
    T data[N];
    
public:
    void process() {
        // Complex template logic that creates code bloat
        for (int i = 0; i < N; i++) {
            if constexpr (UseCache) {
                // Cache-specific logic
                data[i] = static_cast<T>(i * 2);
            } else {
                data[i] = static_cast<T>(i);
            }
            
            if constexpr (UseLogging) {
                // Logging-specific logic (adds to code size)
                cout << "Processing element " << i << ": " << data[i] << endl;
            }
        }
    }
    
    T getSum() const {
        T sum = T{};
        for (int i = 0; i < N; i++) {
            sum += data[i];
        }
        return sum;
    }
};

void demonstrateTemplateInstantiationOverhead() {
    cout << "Starting template instantiation overhead demonstration..." << endl;
    cout << "Monitor compilation time - should see template expansion overhead" << endl;
    
    auto start = chrono::high_resolution_clock::now();
    
    // PERFORMANCE ISSUE: Many different template instantiations
    ExpensiveTemplate<int, 100, true, false> t1;
    ExpensiveTemplate<int, 100, false, false> t2;
    ExpensiveTemplate<int, 100, true, true> t3;
    ExpensiveTemplate<float, 100, true, false> t4;
    ExpensiveTemplate<float, 100, false, false> t5;
    ExpensiveTemplate<double, 100, true, false> t6;
    ExpensiveTemplate<long, 100, true, false> t7;
    ExpensiveTemplate<short, 100, true, false> t8;
    
    // Each instantiation creates separate code - code bloat
    t1.process();
    t2.process(); 
    t3.process();
    t4.process();
    t5.process();
    t6.process();
    t7.process();
    t8.process();
    
    auto sum1 = t1.getSum();
    auto sum2 = t2.getSum();
    auto sum3 = t3.getSum();
    auto sum4 = t4.getSum();
    auto sum5 = t5.getSum();
    auto sum6 = t6.getSum();
    auto sum7 = t7.getSum();
    auto sum8 = t8.getSum();
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Template instantiation overhead completed in: " << duration.count() << " ms" << endl;
    cout << "Sums: " << sum1 << ", " << sum2 << ", " << sum3 << ", " << sum4 << endl;
    cout << "Multiple template instantiations created code bloat" << endl;
}

int main() {
    cout << "Starting template instantiation performance demonstration..." << endl;
    cout << "Task: Multiple template instantiations creating code bloat" << endl;
    cout << "Monitor compilation time and binary size" << endl << endl;
    
    demonstrateTemplateInstantiationOverhead();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check compilation metrics for:" << endl;
    cout << "- Template instantiation compile time" << endl;
    cout << "- Code size growth from template instantiations" << endl;
    cout << "- Binary bloat from duplicate template code" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR TYPE ERASURE)
================================================================================

#include <iostream>
#include <chrono>
#include <vector>
#include <memory>
using namespace std;

// CORREÇÃO: Base class for type erasure
class ProcessorBase {
public:
    virtual ~ProcessorBase() = default;
    virtual void process() = 0;
    virtual double getSum() const = 0;
};

// CORREÇÃO: Single template implementation with type erasure
template<typename T>
class TypedProcessor : public ProcessorBase {
private:
    vector<T> data;
    bool useCache;
    bool useLogging;
    
public:
    TypedProcessor(int size, bool cache = true, bool logging = false) 
        : data(size), useCache(cache), useLogging(logging) {}
    
    void process() override {
        for (size_t i = 0; i < data.size(); i++) {
            if (useCache) {
                data[i] = static_cast<T>(i * 2);
            } else {
                data[i] = static_cast<T>(i);
            }
            
            if (useLogging) {
                cout << "Processing element " << i << ": " << data[i] << endl;
            }
        }
    }
    
    double getSum() const override {
        double sum = 0;
        for (const auto& item : data) {
            sum += static_cast<double>(item);
        }
        return sum;
    }
};

void demonstrateTypeErasure() {
    cout << "Starting type erasure demonstration..." << endl;
    cout << "Monitor compilation time - should see reduced template overhead" << endl;
    
    auto start = chrono::high_resolution_clock::now();
    
    // CORREÇÃO: Type erasure reduces number of template instantiations
    vector<unique_ptr<ProcessorBase>> processors;
    
    processors.push_back(make_unique<TypedProcessor<int>>(100, true, false));
    processors.push_back(make_unique<TypedProcessor<float>>(100, false, false));
    processors.push_back(make_unique<TypedProcessor<double>>(100, true, false));
    processors.push_back(make_unique<TypedProcessor<long>>(100, true, false));
    
    // Process through common interface - no template instantiation bloat
    vector<double> sums;
    for (auto& processor : processors) {
        processor->process();
        sums.push_back(processor->getSum());
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Type erasure completed in: " << duration.count() << " ms" << endl;
    cout << "Sums: ";
    for (auto sum : sums) {
        cout << sum << " ";
    }
    cout << endl;
    cout << "Type erasure reduced template instantiation overhead" << endl;
}

// CORREÇÃO: Explicit template instantiation to control what gets compiled
class OptimizedProcessor {
public:
    template<typename T>
    static void processArray(vector<T>& data, bool useCache) {
        for (size_t i = 0; i < data.size(); i++) {
            if (useCache) {
                data[i] = static_cast<T>(i * 2);
            } else {
                data[i] = static_cast<T>(i);
            }
        }
    }
    
    template<typename T>
    static T computeSum(const vector<T>& data) {
        T sum = T{};
        for (const auto& item : data) {
            sum += item;
        }
        return sum;
    }
};

// CORREÇÃO: Explicit instantiations - compile only what we need
template void OptimizedProcessor::processArray<int>(vector<int>&, bool);
template void OptimizedProcessor::processArray<float>(vector<float>&, bool);
template void OptimizedProcessor::processArray<double>(vector<double>&, bool);

template int OptimizedProcessor::computeSum<int>(const vector<int>&);
template float OptimizedProcessor::computeSum<float>(const vector<float>&);
template double OptimizedProcessor::computeSum<double>(const vector<double>&);

void demonstrateExplicitInstantiation() {
    cout << "Starting explicit instantiation demonstration..." << endl;
    
    auto start = chrono::high_resolution_clock::now();
    
    // CORREÇÃO: Using explicitly instantiated templates
    vector<int> intData(100);
    vector<float> floatData(100);
    vector<double> doubleData(100);
    
    OptimizedProcessor::processArray(intData, true);
    OptimizedProcessor::processArray(floatData, false);
    OptimizedProcessor::processArray(doubleData, true);
    
    auto intSum = OptimizedProcessor::computeSum(intData);
    auto floatSum = OptimizedProcessor::computeSum(floatData);
    auto doubleSum = OptimizedProcessor::computeSum(doubleData);
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Explicit instantiation completed in: " << duration.count() << " ms" << endl;
    cout << "Sums: " << intSum << ", " << floatSum << ", " << doubleSum << endl;
    cout << "Explicit instantiation controlled template bloat" << endl;
}

int main() {
    cout << "Starting optimized template demonstration..." << endl;
    cout << "Task: Reducing template instantiation overhead" << endl;
    cout << "Monitor compilation time and binary size improvements" << endl << endl;
    
    demonstrateTypeErasure();
    cout << endl;
    demonstrateExplicitInstantiation();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- Type erasure reduces template instantiations" << endl;
    cout << "- Explicit instantiation controls what gets compiled" << endl;
    cout << "- Virtual dispatch trades runtime cost for compile-time savings" << endl;
    cout << "- Smaller binary size and faster compilation" << endl;
    
    return 0;
}

================================================================================
*/
