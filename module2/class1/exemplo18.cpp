/*
================================================================================
ATIVIDADE PRÁTICA 18 - STRING FORMATTING PERFORMANCE (C++)
================================================================================

OBJETIVO:
- Demonstrar overhead de string formatting repeated em loops
- Usar CPU profiler para identificar tempo gasto em sprintf/stringstream
- Otimizar usando buffer reuse e efficient formatting
- Comparar diferentes approaches de string formatting

PROBLEMA:
- Repeated sprintf/stringstream operations são custosas
- Memory allocations em string operations
- CPU Profiler mostrará tempo gasto em formatting functions

SOLUÇÃO:
- Reuse buffers quando possível
- Use more efficient formatting methods
- Pre-allocate string capacity

================================================================================
*/

#include <iostream>
#include <string>
#include <sstream>
#include <chrono>
#include <vector>
#include <iomanip>
using namespace std;

void demonstrateInefficiientStringFormatting() {
    cout << "Starting inefficient string formatting demonstration..." << endl;
    cout << "Monitor CPU profiler - should see time spent in string operations" << endl;
    
    const int ITERATIONS = 50000;
    vector<string> formattedStrings;
    formattedStrings.reserve(ITERATIONS);
    
    auto start = chrono::high_resolution_clock::now();
    
    for (int i = 0; i < ITERATIONS; i++) {
        // PERFORMANCE ISSUE: Creating new stringstream every iteration
        stringstream ss;
        ss << "Record #" << setfill('0') << setw(6) << i 
           << " - Value: " << fixed << setprecision(2) << (i * 3.14159) 
           << " - Status: " << (i % 2 == 0 ? "ACTIVE" : "INACTIVE");
        
        string formatted = ss.str(); // String allocation and copy
        formattedStrings.push_back(formatted);
        
        // PERFORMANCE ISSUE: Another string operation
        string upperCased;
        for (char c : formatted) {
            upperCased += toupper(c); // Character by character concatenation
        }
        
        if (i % 5000 == 0) {
            cout << "Formatted " << i << "/" << ITERATIONS << " strings..." << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Inefficient string formatting completed in: " << duration.count() << " ms" << endl;
    cout << "Total strings created: " << formattedStrings.size() << endl;
    cout << "Example result: " << formattedStrings[100] << endl;
}

int main() {
    cout << "Starting string formatting performance demonstration..." << endl;
    cout << "Task: Formatting many strings with repeated stringstream creation" << endl;
    cout << "Monitor CPU Usage Tool for string formatting overhead" << endl << endl;
    
    demonstrateInefficiientStringFormatting();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check CPU profiler for:" << endl;
    cout << "- Time spent in stringstream operations" << endl;
    cout << "- String allocation and deallocation overhead" << endl;
    cout << "- Character-by-character string building" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR EFFICIENT STRING FORMATTING)
================================================================================

#include <iostream>
#include <string>
#include <sstream>
#include <chrono>
#include <vector>
#include <iomanip>
#include <cstdio>
using namespace std;

void demonstrateEfficientStringFormatting() {
    cout << "Starting efficient string formatting demonstration..." << endl;
    cout << "Monitor CPU profiler - should see reduced string operation overhead" << endl;
    
    const int ITERATIONS = 50000;
    vector<string> formattedStrings;
    formattedStrings.reserve(ITERATIONS);
    
    // CORREÇÃO: Reuse stringstream instead of creating new ones
    stringstream ss;
    
    // CORREÇÃO: Pre-allocate buffer for sprintf
    char buffer[256];
    
    auto start = chrono::high_resolution_clock::now();
    
    for (int i = 0; i < ITERATIONS; i++) {
        // CORREÇÃO: Clear and reuse existing stringstream
        ss.str("");
        ss.clear();
        
        // Option 1: Efficient stringstream reuse
        ss << "Record #" << setfill('0') << setw(6) << i 
           << " - Value: " << fixed << setprecision(2) << (i * 3.14159) 
           << " - Status: " << (i % 2 == 0 ? "ACTIVE" : "INACTIVE");
        
        string formatted = ss.str();
        
        // CORREÇÃO: Reserve capacity for string operations
        string upperCased;
        upperCased.reserve(formatted.length());
        
        // CORREÇÃO: More efficient character transformation
        for (char c : formatted) {
            upperCased.push_back(toupper(c));
        }
        
        formattedStrings.push_back(move(formatted));
        
        if (i % 5000 == 0) {
            cout << "Efficiently formatted " << i << "/" << ITERATIONS << " strings..." << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Efficient string formatting completed in: " << duration.count() << " ms" << endl;
    cout << "Total strings created: " << formattedStrings.size() << endl;
    cout << "Example result: " << formattedStrings[100] << endl;
}

void demonstrateSprintfApproach() {
    cout << "Starting sprintf approach demonstration..." << endl;
    
    const int ITERATIONS = 50000;
    vector<string> formattedStrings;
    formattedStrings.reserve(ITERATIONS);
    
    // CORREÇÃO: Use sprintf for maximum formatting performance
    char buffer[256];
    
    auto start = chrono::high_resolution_clock::now();
    
    for (int i = 0; i < ITERATIONS; i++) {
        // CORREÇÃO: sprintf is often faster than stringstream for simple formatting
        snprintf(buffer, sizeof(buffer), 
                "Record #%06d - Value: %.2f - Status: %s", 
                i, i * 3.14159, (i % 2 == 0 ? "ACTIVE" : "INACTIVE"));
        
        string formatted(buffer);
        
        // Convert to uppercase efficiently
        for (char& c : formatted) {
            c = toupper(c);
        }
        
        formattedStrings.push_back(move(formatted));
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "sprintf approach completed in: " << duration.count() << " ms" << endl;
    cout << "Example result: " << formattedStrings[100] << endl;
}

int main() {
    cout << "Starting optimized string formatting demonstration..." << endl;
    cout << "Task: Efficient string formatting with buffer reuse" << endl;
    cout << "Monitor CPU Usage Tool for improved performance" << endl << endl;
    
    demonstrateEfficientStringFormatting();
    cout << endl;
    demonstrateSprintfApproach();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- Stringstream reuse eliminates repeated construction" << endl;
    cout << "- String capacity reservation reduces reallocations" << endl;
    cout << "- sprintf can be faster for simple formatting" << endl;
    cout << "- Move semantics reduce copying" << endl;
    cout << "- In-place character transformation is more efficient" << endl;
    
    return 0;
}

================================================================================
*/
