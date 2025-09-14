/*
================================================================================
ATIVIDADE PRÁTICA 2 - IDENTIFICAÇÃO DE VAZAMENTO DE MEMÓRIA COM HEAP SNAPSHOTS (C#)
================================================================================

OBJETIVO:
- Criar aplicação que faça alocações não-gerenciadas sem liberar memória
- Capturar snapshots de uso da memória com Memory Usage Tool
- Comparar snapshots para identificar crescimento anormal do heap
- Refatorar código para liberar memória corretamente
- Repetir profiling para validar correção

PROBLEMA:
- Funções fazem Marshal.AllocHGlobal mas nunca fazem FreeHGlobal
- Memory Usage Tool mostrará crescimento contínuo do heap não-gerenciado
- Cada iteração adiciona ~600KB sem liberar

SOLUÇÃO:
- Adicionar Marshal.FreeHGlobal para cada AllocHGlobal
- Resultado: estabilização do uso de memória não-gerenciada

================================================================================
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

class Program {
    private static List<IntPtr> leakedPointers = new List<IntPtr>();
    
    static void LeakMemory() {
        IntPtr leak = Marshal.AllocHGlobal(50000 * sizeof(int)); // MEMORY LEAK: Missing Marshal.FreeHGlobal(leak); - Add this line to fix the leak
        leakedPointers.Add(leak);
        
        unsafe {
            int* ptr = (int*)leak.ToPointer();
            for (int i = 0; i < 50000; i++) {
                ptr[i] = i * i;
            }
        }
    }
    
    static void CreateLargeLeak() {
        IntPtr bigLeak = Marshal.AllocHGlobal(25000 * sizeof(double)); // MEMORY LEAK: Missing Marshal.FreeHGlobal(bigLeak); - Add this line to fix the leak
        leakedPointers.Add(bigLeak);
        
        unsafe {
            double* ptr = (double*)bigLeak.ToPointer();
            for (int i = 0; i < 25000; i++) {
                ptr[i] = i * 3.14159;
            }
        }
    }
    
    static void Main() {
        Console.WriteLine("Starting memory leak demonstration...");
        Console.WriteLine("Take heap snapshots at different iterations to see memory growth");
        
        for (int i = 0; i < 100; i++) {
            LeakMemory();
            CreateLargeLeak();
            
            Console.WriteLine($"Iteration: {i + 1} - Heap should be growing...");
            
            if ((i + 1) % 20 == 0) {
                Console.WriteLine($"*** GOOD POINT FOR HEAP SNAPSHOT *** (Iteration {i + 1})");
                Thread.Sleep(2000);
            }
            
            Thread.Sleep(100);
        }
        
        Console.WriteLine("Program finished - memory was never released!");
        
        // TO FIX THE LEAKS, UNCOMMENT THE LINES BELOW:
        // foreach (IntPtr ptr in leakedPointers) {
        //     Marshal.FreeHGlobal(ptr);
        // }
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR A VERSÃO SEM VAZAMENTOS)
================================================================================

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

class Program {
    static void UseMemoryCorrectly() {
        IntPtr data = Marshal.AllocHGlobal(50000 * sizeof(int));
        
        unsafe {
            int* ptr = (int*)data.ToPointer();
            for (int i = 0; i < 50000; i++) {
                ptr[i] = i * i;
            }
        }
        
        // CORREÇÃO: Liberar memória não-gerenciada
        Marshal.FreeHGlobal(data);
    }
    
    static void CreateLargeDataCorrectly() {
        IntPtr bigData = Marshal.AllocHGlobal(25000 * sizeof(double));
        
        unsafe {
            double* ptr = (double*)bigData.ToPointer();
            for (int i = 0; i < 25000; i++) {
                ptr[i] = i * 3.14159;
            }
        }
        
        // CORREÇÃO: Liberar memória não-gerenciada
        Marshal.FreeHGlobal(bigData);
    }
    
    static void Main() {
        Console.WriteLine("Starting corrected memory management demonstration...");
        Console.WriteLine("Take heap snapshots - unmanaged memory usage should remain stable");
        
        for (int i = 0; i < 100; i++) {
            UseMemoryCorrectly();
            CreateLargeDataCorrectly();
            
            Console.WriteLine($"Iteration: {i + 1} - Heap should be stable...");
            
            if ((i + 1) % 20 == 0) {
                Console.WriteLine($"*** GOOD POINT FOR HEAP SNAPSHOT *** (Iteration {i + 1})");
                Thread.Sleep(2000);
            }
            
            Thread.Sleep(100);
        }
        
        Console.WriteLine("Program finished - all memory was properly released!");
    }
}

================================================================================
*/
