/*
================================================================================
ATIVIDADE PRÁTICA 11 - REGEX COMPILATION PERFORMANCE (C++)
================================================================================

OBJETIVO:
- Demonstrar overhead de recompilar regex patterns repetidamente
- Usar CPU profiler para identificar tempo gasto em regex compilation
- Otimizar compilando regex uma vez e reutilizando
- Medir impacto da compilation vs match performance

PROBLEMA:
- Recompilar regex patterns em loops é extremamente custoso
- std::regex constructor é uma operação pesada
- CPU Profiler mostrará tempo gasto em regex compilation

SOLUÇÃO:
- Compilar regex uma vez e reutilizar para múltiplos matches
- Usar std::regex como variável estática ou membro da classe

================================================================================
*/

#include <iostream>
#include <regex>
#include <chrono>
#include <vector>
#include <string>
using namespace std;

void demonstrateRegexRecompilation() {
    cout << "Starting regex recompilation demonstration..." << endl;
    cout << "Monitor CPU profiler - should see time spent in regex compilation" << endl;
    
    const int MATCH_COUNT = 10000;
    vector<string> testStrings = {
        "user@example.com",
        "invalid.email",
        "test@domain.org",
        "notanemail",
        "another@test.com"
    };
    
    auto start = chrono::high_resolution_clock::now();
    
    int validEmails = 0;
    for (int i = 0; i < MATCH_COUNT; i++) {
        string testString = testStrings[i % testStrings.size()];
        
        // PERFORMANCE ISSUE: Recompiling regex pattern every iteration
        regex emailPattern(R"([a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})");
        
        if (regex_match(testString, emailPattern)) {
            validEmails++;
        }
        
        if (i % 1000 == 0) {
            cout << "Completed " << i << "/" << MATCH_COUNT << " regex compilations..." << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Regex recompilation completed in: " << duration.count() << " ms" << endl;
    cout << "Valid emails found: " << validEmails << "/" << MATCH_COUNT << endl;
    cout << "Regex compilations performed: " << MATCH_COUNT << endl;
}

int main() {
    cout << "Starting regex performance demonstration..." << endl;
    cout << "Task: Validating email addresses with regex patterns" << endl;
    cout << "Monitor CPU Usage Tool for regex compilation overhead" << endl << endl;
    
    demonstrateRegexRecompilation();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check CPU profiler for:" << endl;
    cout << "- Time spent in std::regex constructor" << endl;
    cout << "- Pattern compilation overhead" << endl;
    cout << "- Repeated expensive compilation operations" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR A VERSÃO OTIMIZADA)
================================================================================

#include <iostream>
#include <regex>
#include <chrono>
#include <vector>
#include <string>
using namespace std;

void demonstratePrecompiledRegex() {
    cout << "Starting precompiled regex demonstration..." << endl;
    cout << "Monitor CPU profiler - should see reduced compilation overhead" << endl;
    
    const int MATCH_COUNT = 10000;
    vector<string> testStrings = {
        "user@example.com",
        "invalid.email", 
        "test@domain.org",
        "notanemail",
        "another@test.com"
    };
    
    // CORREÇÃO: Compile regex once outside the loop
    static const regex emailPattern(R"([a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})");
    
    auto start = chrono::high_resolution_clock::now();
    
    int validEmails = 0;
    for (int i = 0; i < MATCH_COUNT; i++) {
        string testString = testStrings[i % testStrings.size()];
        
        // CORREÇÃO: Reuse precompiled regex - no compilation overhead
        if (regex_match(testString, emailPattern)) {
            validEmails++;
        }
        
        if (i % 1000 == 0) {
            cout << "Completed " << i << "/" << MATCH_COUNT << " regex matches..." << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Precompiled regex completed in: " << duration.count() << " ms" << endl;
    cout << "Valid emails found: " << validEmails << "/" << MATCH_COUNT << endl;
    cout << "Regex compilations performed: 1 (reused " << MATCH_COUNT << " times)" << endl;
}

int main() {
    cout << "Starting optimized regex demonstration..." << endl;
    cout << "Task: Validating emails with precompiled regex" << endl;
    cout << "Monitor CPU Usage Tool for improved performance" << endl << endl;
    
    demonstratePrecompiledRegex();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- Single regex compilation vs repeated compilation" << endl;
    cout << "- Dramatically reduced CPU usage" << endl;
    cout << "- Focus on actual pattern matching vs compilation" << endl;
    cout << "- Better scalability for high-volume matching" << endl;
    
    return 0;
}

================================================================================
*/
