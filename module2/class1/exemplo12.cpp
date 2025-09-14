/*
================================================================================
ATIVIDADE PRÁTICA 12 - RESOURCE LEAKS E RAII (C++)
================================================================================

OBJETIVO:
- Demonstrar vazamentos de recursos do sistema (handles, sockets)
- Usar system resource monitors para identificar leaks
- Otimizar usando RAII (Resource Acquisition Is Initialization)
- Medir impacto de resource leaks no sistema

PROBLEMA:
- Não fechar handles/recursos causa system resource leaks
- malloc/new sem free/delete causa memory leaks
- Resource monitoring mostrará crescimento de handles

SOLUÇÃO:
- Usar RAII pattern com smart pointers
- Automatic cleanup em destructors

================================================================================
*/

#include <iostream>
#include <chrono>
#include <vector>
#include <fstream>
#include <memory>
using namespace std;

void demonstrateResourceLeaks() {
    cout << "Starting resource leak demonstration..." << endl;
    cout << "Monitor system resources - should see growing resource usage" << endl;
    
    const int ITERATIONS = 500;
    vector<FILE*> leakedFiles;
    vector<int*> leakedMemory;
    
    auto start = chrono::high_resolution_clock::now();
    
    for (int i = 0; i < ITERATIONS; i++) {
        // PERFORMANCE ISSUE: Opening files without closing them
        FILE* file = fopen("temp.txt", "w");
        if (file) {
            fprintf(file, "Test data %d", i);
            fflush(file);
            leakedFiles.push_back(file);
            // Missing fclose(file) - file handle leak!
        }
        
        // PERFORMANCE ISSUE: Allocating memory without freeing
        int* buffer = new int[1000];
        for (int j = 0; j < 1000; j++) {
            buffer[j] = i * j;
        }
        leakedMemory.push_back(buffer);
        // Missing delete[] buffer - memory leak!
        
        // PERFORMANCE ISSUE: Creating ofstream without proper cleanup
        ofstream* stream = new ofstream("temp2.txt");
        *stream << "Data: " << i << endl;
        // Missing delete stream and stream->close() - resource leak!
        
        if (i % 50 == 0) {
            cout << "Created " << i << "/" << ITERATIONS << " leaked resources..." << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Resource leak test completed in: " << duration.count() << " ms" << endl;
    cout << "File handles leaked: " << leakedFiles.size() << endl;
    cout << "Memory blocks leaked: " << leakedMemory.size() << endl;
    cout << "Stream objects leaked: " << ITERATIONS << endl;
}

int main() {
    cout << "Starting resource management demonstration..." << endl;
    cout << "Task: Creating system resources without proper cleanup" << endl;
    cout << "Monitor system resources and memory usage" << endl << endl;
    
    demonstrateResourceLeaks();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check resource monitors for:" << endl;
    cout << "- Growing file handle count" << endl;
    cout << "- Memory usage increase" << endl;
    cout << "- System resource consumption" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR RAII PATTERN)
================================================================================

#include <iostream>
#include <chrono>
#include <vector>
#include <fstream>
#include <memory>
using namespace std;

// CORREÇÃO: RAII wrapper for FILE*
class FileWrapper {
    FILE* file;
public:
    explicit FileWrapper(const char* filename, const char* mode) : file(fopen(filename, mode)) {}
    
    ~FileWrapper() {
        if (file) {
            fclose(file); // Automatic cleanup
        }
    }
    
    FILE* get() const { return file; }
    bool valid() const { return file != nullptr; }
};

void demonstrateRAII() {
    cout << "Starting RAII demonstration..." << endl;
    cout << "Monitor system resources - should remain stable" << endl;
    
    const int ITERATIONS = 500;
    
    auto start = chrono::high_resolution_clock::now();
    
    for (int i = 0; i < ITERATIONS; i++) {
        // CORREÇÃO: RAII ensures automatic file closure
        {
            FileWrapper file("temp.txt", "w");
            if (file.valid()) {
                fprintf(file.get(), "Test data %d", i);
                fflush(file.get());
            }
        } // Automatic fclose() in destructor
        
        // CORREÇÃO: Smart pointer ensures automatic memory cleanup
        {
            auto buffer = make_unique<int[]>(1000);
            for (int j = 0; j < 1000; j++) {
                buffer[j] = i * j;
            }
        } // Automatic delete[] in unique_ptr destructor
        
        // CORREÇÃO: Stack-allocated ofstream with automatic cleanup
        {
            ofstream stream("temp2.txt");
            stream << "Data: " << i << endl;
        } // Automatic close() and destructor cleanup
        
        if (i % 50 == 0) {
            cout << "Processed " << i << "/" << ITERATIONS << " resources with RAII..." << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "RAII demonstration completed in: " << duration.count() << " ms" << endl;
    cout << "No resource leaks - all resources automatically cleaned up" << endl;
}

int main() {
    cout << "Starting optimized RAII demonstration..." << endl;
    cout << "Task: Creating system resources with automatic cleanup" << endl;
    cout << "Monitor system resources for stable usage" << endl << endl;
    
    demonstrateRAII();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- No resource leaks due to RAII pattern" << endl;
    cout << "- Automatic cleanup in destructors" << endl;
    cout << "- Exception-safe resource management" << endl;
    cout << "- Better system resource utilization" << endl;
    
    return 0;
}

================================================================================
*/
