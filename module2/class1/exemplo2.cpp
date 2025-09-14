/*
================================================================================
ATIVIDADE PRÁTICA 2 - IDENTIFICAÇÃO DE VAZAMENTO DE MEMÓRIA COM HEAP SNAPSHOTS (C++)
================================================================================

OBJETIVO:
- Criar aplicação que faça alocações dinâmicas sem liberar memória
- Capturar snapshots de uso da memória com Memory Usage Tool
- Comparar snapshots para identificar crescimento anormal do heap
- Refatorar código para liberar memória corretamente
- Repetir profiling para validar correção

PROBLEMA:
- Funções fazem new[] mas nunca fazem delete[]
- Memory Usage Tool mostrará crescimento contínuo do heap
- Cada iteração adiciona ~600KB sem liberar

SOLUÇÃO:
- Adicionar delete[] para cada new[]
- Resultado: estabilização do uso de memória

================================================================================
*/

#include <iostream>
#include <chrono>
#include <thread>
using namespace std;

void leak_memory() {
    int* leak = new int[50000]; // MEMORY LEAK: Missing delete[] leak; - Add this line to fix the leak
    for (int i = 0; i < 50000; ++i) {
        leak[i] = i * i;
    }
}

void create_large_leak() {
    double* big_leak = new double[25000]; // MEMORY LEAK: Missing delete[] big_leak; - Add this line to fix the leak
    for (int i = 0; i < 25000; ++i) {
        big_leak[i] = i * 3.14159;
    }
}

int main() {
    cout << "Starting memory leak demonstration..." << endl;
    cout << "Take heap snapshots at different iterations to see memory growth" << endl;
    
    for (int i = 0; i < 100; ++i) {
        leak_memory();
        create_large_leak();
        
        cout << "Iteration: " << i + 1 << " - Heap should be growing..." << endl;
        
        if ((i + 1) % 20 == 0) {
            cout << "*** GOOD POINT FOR HEAP SNAPSHOT *** (Iteration " << i + 1 << ")" << endl;
            this_thread::sleep_for(chrono::milliseconds(2000));
        }
        
        this_thread::sleep_for(chrono::milliseconds(100));
    }
    
    cout << "Program finished - memory was never released!" << endl;
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR A VERSÃO SEM VAZAMENTOS)
================================================================================

#include <iostream>
#include <chrono>
#include <thread>
using namespace std;

void use_memory_correctly() {
    int* data = new int[50000];
    for (int i = 0; i < 50000; ++i) {
        data[i] = i * i;
    }
    // CORREÇÃO: Liberar memória alocada
    delete[] data;
}

void create_large_data() {
    double* big_data = new double[25000];
    for (int i = 0; i < 25000; ++i) {
        big_data[i] = i * 3.14159;
    }
    // CORREÇÃO: Liberar memória alocada
    delete[] big_data;
}

int main() {
    cout << "Starting corrected memory management demonstration..." << endl;
    cout << "Take heap snapshots - memory usage should remain stable" << endl;
    
    for (int i = 0; i < 100; ++i) {
        use_memory_correctly();
        create_large_data();
        
        cout << "Iteration: " << i + 1 << " - Heap should be stable..." << endl;
        
        if ((i + 1) % 20 == 0) {
            cout << "*** GOOD POINT FOR HEAP SNAPSHOT *** (Iteration " << i + 1 << ")" << endl;
            this_thread::sleep_for(chrono::milliseconds(2000));
        }
        
        this_thread::sleep_for(chrono::milliseconds(100));
    }
    
    cout << "Program finished - all memory was properly released!" << endl;
    return 0;
}

================================================================================
*/

