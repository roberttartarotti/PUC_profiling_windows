/*
================================================================================
ATIVIDADE PRÁTICA 7 - PERFORMANCE DE ESCRITA EM DISCO (C#)
================================================================================

OBJETIVO:
- Demonstrar ineficiência de escritas byte-a-byte no disco
- Usar I/O profiling tools para identificar gargalos de disco
- Otimizar usando BufferedStream
- Medir ganho de performance em operações de I/O

PROBLEMA:
- Escritas individuais de bytes causam múltiplas syscalls
- FileStream.WriteByte() é uma operação custosa por byte
- I/O Profiler mostrará alta latência e baixo throughput

SOLUÇÃO:
- Usar BufferedStream para acumular dados e escrever em blocos
- Resultado: redução drástica no número de syscalls

================================================================================
*/

using System;
using System.IO;
using System.Diagnostics;

class Program {
    static void InefficientDiskWrite(string filename, int dataSize) {
        Console.WriteLine("Starting inefficient disk write...");
        Console.WriteLine("Monitor I/O performance - should see many small write operations");
        
        var sw = Stopwatch.StartNew();
        
        using (var file = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
            // PERFORMANCE ISSUE: Writing one byte at a time causes excessive I/O operations
            for (int i = 0; i < dataSize; i++) {
                byte value = (byte)(i % 256);
                file.WriteByte(value); // Each write is potentially a separate I/O operation
                file.Flush(); // Force immediate write to disk
                
                if (i % 10000 == 0) {
                    Console.WriteLine($"Written {i}/{dataSize} bytes...");
                }
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Inefficient write completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Approximate I/O operations: ~{dataSize} (one per byte)");
        Console.WriteLine();
    }
    
    static void Main() {
        const int DATA_SIZE = 100000;
        const string filename = "test_output.dat";
        
        Console.WriteLine("Starting disk I/O performance demonstration...");
        Console.WriteLine($"Task: Writing {DATA_SIZE} bytes to disk");
        Console.WriteLine("Monitor I/O profiling tools for disk usage patterns");
        Console.WriteLine();
        
        InefficientDiskWrite(filename, DATA_SIZE);
        
        Console.WriteLine("=== I/O PERFORMANCE ANALYSIS ===");
        Console.WriteLine("Check I/O profiler for:");
        Console.WriteLine("- High number of I/O operations");
        Console.WriteLine("- Low I/O throughput");
        Console.WriteLine("- High I/O wait time");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR A VERSÃO OTIMIZADA)
================================================================================

using System;
using System.IO;
using System.Diagnostics;

class Program {
    static void EfficientDiskWrite(string filename, int dataSize) {
        Console.WriteLine("Starting efficient disk write...");
        Console.WriteLine("Monitor I/O performance - should see fewer, larger write operations");
        
        var sw = Stopwatch.StartNew();
        
        using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
        using (var bufferedStream = new BufferedStream(fileStream, 8192)) { // CORREÇÃO: 8KB buffer
            for (int i = 0; i < dataSize; i++) {
                byte value = (byte)(i % 256);
                bufferedStream.WriteByte(value); // Buffered - reduces actual I/O operations
                
                if (i % 10000 == 0) {
                    Console.WriteLine($"Buffered {i}/{dataSize} bytes...");
                }
            }
            // BufferedStream automatically flushes on disposal
        }
        
        sw.Stop();
        
        Console.WriteLine($"Efficient write completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Approximate I/O operations: ~{dataSize / 8192 + 1} (buffered)");
    }
    
    static void Main() {
        const int DATA_SIZE = 100000;
        const string filename = "test_output.dat";
        
        Console.WriteLine("Starting optimized disk I/O demonstration...");
        Console.WriteLine($"Task: Writing {DATA_SIZE} bytes to disk efficiently");
        Console.WriteLine("Monitor I/O profiling tools for improved performance");
        Console.WriteLine();
        
        EfficientDiskWrite(filename, DATA_SIZE);
        
        Console.WriteLine("=== OPTIMIZED I/O RESULTS ===");
        Console.WriteLine("Improvements:");
        Console.WriteLine("- Dramatically fewer I/O operations");
        Console.WriteLine("- Higher I/O throughput");
        Console.WriteLine("- Reduced I/O wait time");
    }
}

================================================================================
*/
