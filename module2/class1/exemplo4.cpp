/*
================================================================================
ATIVIDADE PRÁTICA 4 - HOTSPOT EM ALGORITMO DE ORDENAÇÃO (C++)
================================================================================

OBJETIVO:
- Implementar algoritmo de ordenação simples (Bubble Sort) em grande vetor
- Usar CPU Usage Tool para identificar impacto do algoritmo e pontos quentes
- Substituir por QuickSort ou algoritmo mais eficiente
- Comparar perfis e medir ganho de performance

PROBLEMA:
- Bubble Sort possui complexidade O(n²) com loops aninhados
- CPU Usage Tool mostrará hotspot nos loops aninhados da função bubbleSort
- Para 25.000 elementos: ~625 milhões de operações teóricas

SOLUÇÃO:
- Implementar QuickSort com complexidade O(n log n)
- Resultado: redução de 625M para ~367K operações (1000x+ melhoria)

================================================================================
*/

#include <iostream>
#include <chrono>
#include <random>
#include <algorithm>
using namespace std;

void bubbleSort(int arr[], int n) {
    for (int i = 0; i < n-1; i++) {
        for (int j = 0; j < n-i-1; j++) { // CPU HOTSPOT: O(n²) nested loops - Replace with QuickSort O(n log n) for better performance
            if (arr[j] > arr[j+1]) {
                swap(arr[j], arr[j+1]);
            }
        }
        
        if (i % 1000 == 0) {
            cout << "Bubble Sort Progress: " << i << "/" << (n-1) << " passes completed" << endl;
        }
    }
}

int main() {
    const int ARRAY_SIZE = 25000;
    cout << "Starting CPU hotspot demonstration with Bubble Sort..." << endl;
    cout << "Array size: " << ARRAY_SIZE << " elements" << endl;
    cout << "Monitor CPU usage - Bubble Sort will show O(n²) complexity hotspot" << endl;
    
    int* arr = new int[ARRAY_SIZE];
    
    random_device rd;
    mt19937 gen(rd());
    uniform_int_distribution<> dis(1, 100000);
    
    for (int i = 0; i < ARRAY_SIZE; i++) {
        arr[i] = dis(gen);
    }
    
    cout << "Array generated with random values" << endl;
    cout << "Starting Bubble Sort..." << endl;
    
    auto start = chrono::high_resolution_clock::now();
    
    bubbleSort(arr, ARRAY_SIZE);
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << endl;
    cout << "=== SORTING COMPLETED ===" << endl;
    cout << "Bubble Sort execution time: " << duration.count() << " ms" << endl;
    cout << "Algorithm complexity: O(n²) - " << (ARRAY_SIZE * ARRAY_SIZE / 1000000.0) << "M theoretical operations" << endl;
    
    bool isSorted = is_sorted(arr, arr + ARRAY_SIZE);
    cout << "Array is " << (isSorted ? "correctly sorted" : "NOT sorted") << endl;
    
    delete[] arr;
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR QUICKSORT OTIMIZADO)
================================================================================

#include <iostream>
#include <chrono>
#include <random>
#include <algorithm>
using namespace std;

int partition(int arr[], int low, int high) {
    int pivot = arr[high];
    int i = (low - 1);
    
    for (int j = low; j <= high - 1; j++) {
        if (arr[j] < pivot) {
            i++;
            swap(arr[i], arr[j]);
        }
    }
    swap(arr[i + 1], arr[high]);
    return (i + 1);
}

void quickSort(int arr[], int low, int high) {
    if (low < high) {
        int pi = partition(arr, low, high);
        quickSort(arr, low, pi - 1);  // CORREÇÃO: O(n log n) complexity instead of O(n²)
        quickSort(arr, pi + 1, high);
    }
}

int main() {
    const int ARRAY_SIZE = 25000;
    cout << "Starting optimized sorting demonstration with QuickSort..." << endl;
    cout << "Array size: " << ARRAY_SIZE << " elements" << endl;
    cout << "Monitor CPU usage - QuickSort will show O(n log n) complexity" << endl;
    
    int* arr = new int[ARRAY_SIZE];
    
    random_device rd;
    mt19937 gen(rd());
    uniform_int_distribution<> dis(1, 100000);
    
    for (int i = 0; i < ARRAY_SIZE; i++) {
        arr[i] = dis(gen);
    }
    
    cout << "Array generated with random values" << endl;
    cout << "Starting QuickSort..." << endl;
    
    auto start = chrono::high_resolution_clock::now();
    
    quickSort(arr, 0, ARRAY_SIZE - 1);
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    cout << endl;
    cout << "=== SORTING COMPLETED ===" << endl;
    cout << "QuickSort execution time: " << duration.count() << " ms" << endl;
    cout << "Algorithm complexity: O(n log n) - " << (ARRAY_SIZE * log2(ARRAY_SIZE) / 1000.0) << "K theoretical operations" << endl;
    
    bool isSorted = is_sorted(arr, arr + ARRAY_SIZE);
    cout << "Array is " << (isSorted ? "correctly sorted" : "NOT sorted") << endl;
    
    delete[] arr;
    return 0;
}

================================================================================
*/