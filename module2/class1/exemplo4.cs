/*
================================================================================
ATIVIDADE PRÁTICA 4 - HOTSPOT EM ALGORITMO DE ORDENAÇÃO (C#)
================================================================================

OBJETIVO:
- Implementar algoritmo de ordenação simples (Bubble Sort) em grande array
- Usar CPU Usage Tool para identificar impacto do algoritmo e pontos quentes
- Substituir por QuickSort ou algoritmo mais eficiente
- Comparar perfis e medir ganho de performance

PROBLEMA:
- Bubble Sort possui complexidade O(n²) com loops aninhados
- CPU Usage Tool mostrará hotspot nos loops aninhados do método BubbleSort
- Para 25.000 elementos: ~625 milhões de operações teóricas

SOLUÇÃO:
- Implementar QuickSort com complexidade O(n log n)
- Resultado: redução de 625M para ~367K operações (1000x+ melhoria)

================================================================================
*/

using System;
using System.Diagnostics;

class Program {
    static void BubbleSort(int[] arr) {
        int n = arr.Length;
        for (int i = 0; i < n - 1; i++) {
            for (int j = 0; j < n - i - 1; j++) { // CPU HOTSPOT: O(n²) nested loops - Replace with QuickSort O(n log n) for better performance
                if (arr[j] > arr[j + 1]) {
                    (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
                }
            }
            
            if (i % 1000 == 0) {
                Console.WriteLine($"Bubble Sort Progress: {i}/{n-1} passes completed");
            }
        }
    }
    
    static void Main() {
        const int ARRAY_SIZE = 25000;
        Console.WriteLine("Starting CPU hotspot demonstration with Bubble Sort...");
        Console.WriteLine($"Array size: {ARRAY_SIZE} elements");
        Console.WriteLine("Monitor CPU usage - Bubble Sort will show O(n²) complexity hotspot");
        
        int[] arr = new int[ARRAY_SIZE];
        Random rnd = new Random();
        
        for (int i = 0; i < ARRAY_SIZE; i++) {
            arr[i] = rnd.Next(1, 100001);
        }
        
        Console.WriteLine("Array generated with random values");
        Console.WriteLine("Starting Bubble Sort...");
        
        var sw = Stopwatch.StartNew();
        
        BubbleSort(arr);
        
        sw.Stop();
        
        Console.WriteLine();
        Console.WriteLine("=== SORTING COMPLETED ===");
        Console.WriteLine($"Bubble Sort execution time: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Algorithm complexity: O(n²) - {(ARRAY_SIZE * ARRAY_SIZE / 1000000.0):F1}M theoretical operations");
        
        bool isSorted = true;
        for (int i = 0; i < ARRAY_SIZE - 1; i++) {
            if (arr[i] > arr[i + 1]) {
                isSorted = false;
                break;
            }
        }
        Console.WriteLine($"Array is {(isSorted ? "correctly sorted" : "NOT sorted")}");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR QUICKSORT OTIMIZADO)
================================================================================

using System;
using System.Diagnostics;

class Program {
    static int Partition(int[] arr, int low, int high) {
        int pivot = arr[high];
        int i = (low - 1);
        
        for (int j = low; j <= high - 1; j++) {
            if (arr[j] < pivot) {
                i++;
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
        }
        (arr[i + 1], arr[high]) = (arr[high], arr[i + 1]);
        return (i + 1);
    }
    
    static void QuickSort(int[] arr, int low, int high) {
        if (low < high) {
            int pi = Partition(arr, low, high);
            QuickSort(arr, low, pi - 1);  // CORREÇÃO: O(n log n) complexity instead of O(n²)
            QuickSort(arr, pi + 1, high);
        }
    }
    
    static void Main() {
        const int ARRAY_SIZE = 25000;
        Console.WriteLine("Starting optimized sorting demonstration with QuickSort...");
        Console.WriteLine($"Array size: {ARRAY_SIZE} elements");
        Console.WriteLine("Monitor CPU usage - QuickSort will show O(n log n) complexity");
        
        int[] arr = new int[ARRAY_SIZE];
        Random rnd = new Random();
        
        for (int i = 0; i < ARRAY_SIZE; i++) {
            arr[i] = rnd.Next(1, 100001);
        }
        
        Console.WriteLine("Array generated with random values");
        Console.WriteLine("Starting QuickSort...");
        
        var sw = Stopwatch.StartNew();
        
        QuickSort(arr, 0, ARRAY_SIZE - 1);
        
        sw.Stop();
        
        Console.WriteLine();
        Console.WriteLine("=== SORTING COMPLETED ===");
        Console.WriteLine($"QuickSort execution time: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Algorithm complexity: O(n log n) - {(ARRAY_SIZE * Math.Log2(ARRAY_SIZE) / 1000.0):F1}K theoretical operations");
        
        bool isSorted = true;
        for (int i = 0; i < ARRAY_SIZE - 1; i++) {
            if (arr[i] > arr[i + 1]) {
                isSorted = false;
                break;
            }
        }
        Console.WriteLine($"Array is {(isSorted ? "correctly sorted" : "NOT sorted")}");
    }
}

================================================================================
*/