/*
================================================================================
ATIVIDADE PRÁTICA 8 - EXCEPTION HANDLING PARA CONTROLE DE FLUXO (C++)
================================================================================

OBJETIVO:
- Demonstrar uso ineficiente de exceptions para controle de fluxo
- Usar CPU profiler para identificar overhead de exception handling
- Otimizar usando retorno de códigos de erro ou std::optional
- Medir impacto das exceptions na performance

PROBLEMA:
- Exceptions são custosas quando usadas para controle de fluxo normal
- Stack unwinding e cleanup são operações pesadas
- CPU Profiler mostrará tempo gasto em exception handling

SOLUÇÃO:
- Usar exceptions apenas para erros excepcionais
- Implementar validação de dados com return codes

================================================================================
*/

#include <iostream>
#include <chrono>
#include <stdexcept>
using namespace std;

class DataProcessor {
public:
    // PERFORMANCE ISSUE: Using exceptions for normal control flow
    int processWithExceptions(int value) {
        if (value < 0) {
            throw invalid_argument("Negative value not allowed"); // Exception for control flow - expensive!
        }
        if (value > 1000) {
            throw out_of_range("Value too large"); // Another control flow exception
        }
        if (value % 2 == 0) {
            throw runtime_error("Even numbers not supported"); // Normal business logic as exception
        }
        
        return value * 2;
    }
};

void demonstrateExceptionOverhead() {
    cout << "Starting exception-heavy processing..." << endl;
    cout << "Monitor CPU profiler - should see overhead in exception handling" << endl;
    
    DataProcessor processor;
    int successfulOperations = 0;
    int totalOperations = 100000;
    
    auto start = chrono::high_resolution_clock::now();
    
    for (int i = 0; i < totalOperations; i++) {
        try {
            int result = processor.processWithExceptions(i % 1500);
            successfulOperations++;
        }
        catch (const exception& e) {
            // Exception handling overhead occurs here frequently
            continue; // Using exceptions for normal control flow
        }
        
        if (i % 10000 == 0) {
            cout << "Processed " << i << "/" << totalOperations << " values..." << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Exception-based processing completed in: " << duration.count() << " ms" << endl;
    cout << "Successful operations: " << successfulOperations << "/" << totalOperations << endl;
    cout << "Exceptions thrown: " << (totalOperations - successfulOperations) << endl;
}

int main() {
    cout << "Starting exception handling performance demonstration..." << endl;
    cout << "Task: Processing data with exception-based validation" << endl;
    cout << "Monitor CPU Usage Tool for exception handling overhead" << endl << endl;
    
    demonstrateExceptionOverhead();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check CPU profiler for:" << endl;
    cout << "- Time spent in exception construction/destruction" << endl;
    cout << "- Stack unwinding overhead" << endl;
    cout << "- Exception handler dispatch time" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR A VERSÃO OTIMIZADA)
================================================================================

#include <iostream>
#include <chrono>
#include <optional>
using namespace std;

enum class ProcessResult {
    Success,
    NegativeValue,
    ValueTooLarge,
    EvenNumber
};

class OptimizedDataProcessor {
public:
    // CORREÇÃO: Using return codes instead of exceptions for control flow
    pair<ProcessResult, optional<int>> processWithReturnCodes(int value) {
        if (value < 0) {
            return {ProcessResult::NegativeValue, nullopt}; // Fast return - no exception overhead
        }
        if (value > 1000) {
            return {ProcessResult::ValueTooLarge, nullopt}; // Fast return
        }
        if (value % 2 == 0) {
            return {ProcessResult::EvenNumber, nullopt}; // Fast return
        }
        
        return {ProcessResult::Success, value * 2};
    }
};

void demonstrateOptimizedProcessing() {
    cout << "Starting optimized processing..." << endl;
    cout << "Monitor CPU profiler - should see reduced overhead" << endl;
    
    OptimizedDataProcessor processor;
    int successfulOperations = 0;
    int totalOperations = 100000;
    
    auto start = chrono::high_resolution_clock::now();
    
    for (int i = 0; i < totalOperations; i++) {
        auto [result, value] = processor.processWithReturnCodes(i % 1500);
        
        if (result == ProcessResult::Success && value) {
            successfulOperations++;
            // Process the successful result
        }
        // No exception handling overhead - just fast conditional checks
        
        if (i % 10000 == 0) {
            cout << "Processed " << i << "/" << totalOperations << " values..." << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Optimized processing completed in: " << duration.count() << " ms" << endl;
    cout << "Successful operations: " << successfulOperations << "/" << totalOperations << endl;
    cout << "No exceptions thrown - using return codes for flow control" << endl;
}

int main() {
    cout << "Starting optimized exception handling demonstration..." << endl;
    cout << "Task: Processing data with return-code-based validation" << endl;
    cout << "Monitor CPU Usage Tool for improved performance" << endl << endl;
    
    demonstrateOptimizedProcessing();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- No exception construction/destruction overhead" << endl;
    cout << "- No stack unwinding costs" << endl;
    cout << "- Fast conditional logic for flow control" << endl;
    
    return 0;
}

================================================================================
*/
