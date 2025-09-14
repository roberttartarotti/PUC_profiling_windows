/*
================================================================================
ATIVIDADE PRÁTICA 7 - PERFORMANCE DE ESCRITA EM DISCO (C++)
================================================================================

OBJETIVO:
- Demonstrar ineficiência de escritas byte-a-byte no disco
- Usar I/O profiling tools para identificar gargalos de disco
- Otimizar usando buffer de escrita
- Medir ganho de performance em operações de I/O

PROBLEMA:
- Escritas individuais de bytes causam múltiplas syscalls
- Cada write() é uma operação custosa do sistema operacional
- I/O Profiler mostrará alta latência e baixo throughput

SOLUÇÃO:
- Usar buffer para acumular dados e escrever em blocos maiores
- Resultado: redução drástica no número de syscalls

================================================================================
*/

#include <iostream>
#include <fstream>
#include <chrono>
#include <string>
using namespace std;

void inefficientDiskWrite(const string& filename, int dataSize) {
    cout << "Starting inefficient disk write..." << endl;
    cout << "Monitor I/O performance - should see many small write operations" << endl;
    
    auto start = chrono::high_resolution_clock::now();
    
    ofstream file(filename, ios::binary);
    if (!file.is_open()) {
        cerr << "Failed to open file!" << endl;
        return;
    }
    
    // PERFORMANCE ISSUE: Writing one byte at a time causes excessive syscalls
    for (int i = 0; i < dataSize; i++) {
        char byte = static_cast<char>(i % 256);
        file.write(&byte, 1); // Each write is a separate syscall - very inefficient!
        file.flush(); // Force immediate write to disk
        
        if (i % 10000 == 0) {
            cout << "Written " << i << "/" << dataSize << " bytes..." << endl;
        }
    }
    
    file.close();
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Inefficient write completed in: " << duration.count() << " ms" << endl;
    cout << "Total syscalls: ~" << dataSize << " (one per byte)" << endl << endl;
}

int main() {
    const int DATA_SIZE = 100000;
    const string filename = "test_output.dat";
    
    cout << "Starting disk I/O performance demonstration..." << endl;
    cout << "Task: Writing " << DATA_SIZE << " bytes to disk" << endl;
    cout << "Monitor I/O profiling tools for disk usage patterns" << endl << endl;
    
    inefficientDiskWrite(filename, DATA_SIZE);
    
    cout << "=== I/O PERFORMANCE ANALYSIS ===" << endl;
    cout << "Check I/O profiler for:" << endl;
    cout << "- High number of write syscalls" << endl;
    cout << "- Low I/O throughput" << endl;
    cout << "- High I/O wait time" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR A VERSÃO OTIMIZADA)
================================================================================

#include <iostream>
#include <fstream>
#include <chrono>
#include <string>
#include <vector>
using namespace std;

void efficientDiskWrite(const string& filename, int dataSize) {
    cout << "Starting efficient disk write..." << endl;
    cout << "Monitor I/O performance - should see fewer, larger write operations" << endl;
    
    auto start = chrono::high_resolution_clock::now();
    
    ofstream file(filename, ios::binary);
    if (!file.is_open()) {
        cerr << "Failed to open file!" << endl;
        return;
    }
    
    const int BUFFER_SIZE = 8192; // 8KB buffer
    vector<char> buffer;
    buffer.reserve(BUFFER_SIZE);
    
    // CORREÇÃO: Buffer writes to reduce syscalls
    for (int i = 0; i < dataSize; i++) {
        char byte = static_cast<char>(i % 256);
        buffer.push_back(byte);
        
        // Write buffer when full
        if (buffer.size() >= BUFFER_SIZE) {
            file.write(buffer.data(), buffer.size());
            buffer.clear();
        }
        
        if (i % 10000 == 0) {
            cout << "Buffered " << i << "/" << dataSize << " bytes..." << endl;
        }
    }
    
    // Write remaining data
    if (!buffer.empty()) {
        file.write(buffer.data(), buffer.size());
    }
    
    file.close();
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << "Efficient write completed in: " << duration.count() << " ms" << endl;
    cout << "Total syscalls: ~" << (dataSize / BUFFER_SIZE + 1) << " (buffered)" << endl;
}

int main() {
    const int DATA_SIZE = 100000;
    const string filename = "test_output.dat";
    
    cout << "Starting optimized disk I/O demonstration..." << endl;
    cout << "Task: Writing " << DATA_SIZE << " bytes to disk efficiently" << endl;
    cout << "Monitor I/O profiling tools for improved performance" << endl << endl;
    
    efficientDiskWrite(filename, DATA_SIZE);
    
    cout << "=== OPTIMIZED I/O RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- Dramatically fewer syscalls" << endl;
    cout << "- Higher I/O throughput" << endl;
    cout << "- Reduced I/O wait time" << endl;
    
    return 0;
}

================================================================================
*/
