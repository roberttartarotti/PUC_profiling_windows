
#include <iostream>
#include <sstream>
#include <chrono>
using namespace std;

int main() {
    cout << "Starting optimized string building demonstration..." << endl;
    cout << "Monitor memory allocations - should see much fewer temporary objects" << endl;
    
    auto start = chrono::high_resolution_clock::now();
    
    stringstream ss; // CORREÇÃO: stringstream evita criação de objetos temporários
    for (int i = 0; i < 100000; i++) {
        ss << i << ", ";
        
        if (i % 10000 == 0) {
            string temp = ss.str();
            cout << "Iteration " << i << ": Current string length = " << temp.length() << " characters" << endl;
        }
    }
    
    string result = ss.str();
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << endl;
    cout << "=== FINAL RESULTS ===" << endl;
    cout << "Execution time: " << duration.count() << " ms" << endl;
    cout << "Final string length: " << result.length() << " characters" << endl;
    cout << "Done building strings efficiently!" << endl;
    
    return 0;
}
