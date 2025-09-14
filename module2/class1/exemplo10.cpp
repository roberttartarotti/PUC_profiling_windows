/*
================================================================================
ATIVIDADE PRÁTICA 10 - PERFORMANCE DE COLEÇÕES (C++)
================================================================================

OBJETIVO:
- Demonstrar ineficiência de usar estrutura de dados inadequada
- Usar CPU profiler para identificar gargalos em operações de busca
- Otimizar escolhendo container adequado (vector vs unordered_map)
- Comparar complexidade O(n) vs O(1) em operações de lookup

PROBLEMA:
- Usar std::vector para muitas operações de busca é O(n)
- Linear search em containers grandes é ineficiente
- CPU Profiler mostrará tempo gasto em std::find

SOLUÇÃO:
- Usar std::unordered_map para lookup O(1)
- Escolher container baseado no padrão de uso

================================================================================
*/

#include <iostream>
#include <vector>
#include <unordered_map>
#include <chrono>
#include <algorithm>
#include <random>
using namespace std;

void demonstrateInefficiectLookup() {
    cout << "Starting inefficient vector lookup demonstration..." << endl;
    cout << "Monitor CPU profiler - should see time spent in linear search" << endl;
    
    const int DATA_SIZE = 50000;
    const int LOOKUP_COUNT = 10000;
    
    // Fill vector with data
    vector<int> dataVector;
    for (int i = 0; i < DATA_SIZE; i++) {
        dataVector.push_back(i * 2); // Even numbers
    }
    
    // Random number generator for lookups
    random_device rd;
    mt19937 gen(rd());
    uniform_int_distribution<> dis(0, DATA_SIZE * 2);
    
    auto start = chrono::high_resolution_clock::now();
    
    int foundCount = 0;
    for (int i = 0; i < LOOKUP_COUNT; i++) {
        int searchValue = dis(gen);
        
        // PERFORMANCE ISSUE: Linear search O(n) in vector
        auto it = find(dataVector.begin(), dataVector.end(), searchValue);
        if (it != dataVector.end()) {
            foundCount++;
        }
        
        if (i % 1000 == 0) {
            cout << "Completed " << i << "/" << LOOKUP_COUNT << " linear searches..." << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Vector lookup completed in: " << duration.count() << " ms" << endl;
    cout << "Found " << foundCount << "/" << LOOKUP_COUNT << " values" << endl;
    cout << "Average complexity per lookup: O(" << DATA_SIZE << ") - linear search" << endl;
}

int main() {
    cout << "Starting collection performance demonstration..." << endl;
    cout << "Task: Performing many lookup operations in large dataset" << endl;
    cout << "Monitor CPU Usage Tool for search algorithm performance" << endl << endl;
    
    demonstrateInefficiectLookup();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check CPU profiler for:" << endl;
    cout << "- Time spent in std::find algorithm" << endl;
    cout << "- Linear search pattern in vector iteration" << endl;
    cout << "- High CPU usage due to O(n) complexity" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR A VERSÃO OTIMIZADA)
================================================================================

#include <iostream>
#include <unordered_set>
#include <chrono>
#include <random>
using namespace std;

void demonstrateEfficientLookup() {
    cout << "Starting efficient hash set lookup demonstration..." << endl;
    cout << "Monitor CPU profiler - should see reduced search time" << endl;
    
    const int DATA_SIZE = 50000;
    const int LOOKUP_COUNT = 10000;
    
    // CORREÇÃO: Use unordered_set for O(1) average lookup time
    unordered_set<int> dataSet;
    for (int i = 0; i < DATA_SIZE; i++) {
        dataSet.insert(i * 2); // Even numbers
    }
    
    // Random number generator for lookups
    random_device rd;
    mt19937 gen(rd());
    uniform_int_distribution<> dis(0, DATA_SIZE * 2);
    
    auto start = chrono::high_resolution_clock::now();
    
    int foundCount = 0;
    for (int i = 0; i < LOOKUP_COUNT; i++) {
        int searchValue = dis(gen);
        
        // CORREÇÃO: Hash lookup O(1) average case
        if (dataSet.find(searchValue) != dataSet.end()) {
            foundCount++;
        }
        
        if (i % 1000 == 0) {
            cout << "Completed " << i << "/" << LOOKUP_COUNT << " hash lookups..." << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Hash set lookup completed in: " << duration.count() << " ms" << endl;
    cout << "Found " << foundCount << "/" << LOOKUP_COUNT << " values" << endl;
    cout << "Average complexity per lookup: O(1) - hash lookup" << endl;
}

int main() {
    cout << "Starting optimized collection demonstration..." << endl;
    cout << "Task: Performing lookups using hash-based container" << endl;
    cout << "Monitor CPU Usage Tool for improved search performance" << endl << endl;
    
    demonstrateEfficientLookup();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- O(1) average lookup time vs O(n) linear search" << endl;
    cout << "- Dramatically reduced CPU usage for searches" << endl;
    cout << "- Constant time performance regardless of data size" << endl;
    cout << "- Better scalability for large datasets" << endl;
    
    return 0;
}

================================================================================
*/
