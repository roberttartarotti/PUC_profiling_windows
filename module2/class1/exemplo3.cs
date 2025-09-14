/*
================================================================================
ATIVIDADE PRÁTICA 3 - USO EXCESSIVO DE OBJETOS TEMPORÁRIOS (C#)
================================================================================

OBJETIVO:
- Criar aplicação que concatena strings em loop gerando objetos temporários
- Executar dentro do profiler de memória para detectar alocações intensas e GC frequente
- Refatorar código para usar StringBuilder, reduzindo alocações
- Validar melhorias na redução da coleta de lixo e consumo de memória

PROBLEMA:
- String concatenation com += cria objetos string temporários a cada iteração
- ToString() + ", " gera strings temporárias que são imediatamente coletadas pelo GC
- Memory Profiler mostrará alocações intensas e coletas frequentes de lixo

SOLUÇÃO:
- Usar StringBuilder que gerencia buffer interno de forma eficiente
- Resultado: redução drástica de alocações e coletas de lixo

================================================================================
*/

using System;
using System.Diagnostics;

class Program {
    static void Main() {
        Console.WriteLine("Starting temporary objects demonstration...");
        Console.WriteLine("Monitor memory allocations and GC collections during execution");
        
        var sw = Stopwatch.StartNew();
        long initialMemory = GC.GetTotalMemory(false);
        int initialCollections = GC.CollectionCount(0);
        
        Console.WriteLine($"Initial memory: {initialMemory:N0} bytes");
        Console.WriteLine($"Initial GC collections: {initialCollections}");
        Console.WriteLine();
        
        string result = "";
        for (int i = 0; i < 100000; i++) {
            result += i.ToString() + ", "; // PERFORMANCE ISSUE: Creates temporary string objects - Use StringBuilder instead
            
            if (i % 10000 == 0) {
                long currentMemory = GC.GetTotalMemory(false);
                int currentCollections = GC.CollectionCount(0);
                Console.WriteLine($"Iteration {i:N0}: Memory = {currentMemory:N0} bytes, GC Collections = {currentCollections}");
            }
        }
        
        sw.Stop();
        long finalMemory = GC.GetTotalMemory(false);
        int finalCollections = GC.CollectionCount(0);
        
        Console.WriteLine();
        Console.WriteLine("=== FINAL RESULTS ===");
        Console.WriteLine($"Execution time: {sw.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"Final memory: {finalMemory:N0} bytes");
        Console.WriteLine($"Memory increase: {(finalMemory - initialMemory):N0} bytes");
        Console.WriteLine($"Total GC collections: {(finalCollections - initialCollections)}");
        Console.WriteLine($"Final string length: {result.Length:N0} characters");
        Console.WriteLine("Done concatenating strings - Check memory profiler for temporary object allocations!");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR A VERSÃO OTIMIZADA)
================================================================================

using System;
using System.Text;
using System.Diagnostics;

class Program {
    static void Main() {
        Console.WriteLine("Starting optimized string building demonstration...");
        Console.WriteLine("Monitor memory allocations - should see much fewer temporary objects");
        
        var sw = Stopwatch.StartNew();
        long initialMemory = GC.GetTotalMemory(false);
        int initialCollections = GC.CollectionCount(0);
        
        Console.WriteLine($"Initial memory: {initialMemory:N0} bytes");
        Console.WriteLine($"Initial GC collections: {initialCollections}");
        Console.WriteLine();
        
        StringBuilder sb = new StringBuilder(); // CORREÇÃO: StringBuilder evita criação de objetos temporários
        for (int i = 0; i < 100000; i++) {
            sb.Append(i);
            sb.Append(", ");
            
            if (i % 10000 == 0) {
                long currentMemory = GC.GetTotalMemory(false);
                int currentCollections = GC.CollectionCount(0);
                Console.WriteLine($"Iteration {i:N0}: Memory = {currentMemory:N0} bytes, GC Collections = {currentCollections}");
            }
        }
        
        sw.Stop();
        long finalMemory = GC.GetTotalMemory(false);
        int finalCollections = GC.CollectionCount(0);
        
        Console.WriteLine();
        Console.WriteLine("=== FINAL RESULTS ===");
        Console.WriteLine($"Execution time: {sw.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"Final memory: {finalMemory:N0} bytes");
        Console.WriteLine($"Memory increase: {(finalMemory - initialMemory):N0} bytes");
        Console.WriteLine($"Total GC collections: {(finalCollections - initialCollections)}");
        Console.WriteLine($"Final string length: {sb.Length:N0} characters");
        Console.WriteLine("Done building strings efficiently!");
    }
}

================================================================================
*/