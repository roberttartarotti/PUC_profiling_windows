/*
================================================================================
ATIVIDADE PRÁTICA 25 - STRING INTERNING PERFORMANCE (C#)
================================================================================

OBJETIVO:
- Demonstrar overhead de string operations without interning
- Usar Memory profiler para identificar string allocation patterns
- Otimizar usando string interning e StringBuilder
- Medir impacto de repeated string operations

PROBLEMA:
- String concatenation cria many temporary objects
- Repeated equal strings não são reused
- Memory profiler mostrará excessive string allocations

SOLUÇÃO:
- String interning para reuse
- StringBuilder para efficient concatenation
- ReadOnlySpan<char> para avoid allocations

================================================================================
*/

using System;
using System.Diagnostics;
using System.Collections.Generic;

class Program {
    static void DemonstrateStringAllocationOverhead() {
        Console.WriteLine("Starting string allocation overhead demonstration...");
        Console.WriteLine("Monitor Memory profiler - should see excessive string allocations");
        
        const int ITERATIONS = 100000;
        var strings = new List<string>();
        
        long initialMemory = GC.GetTotalMemory(false);
        int initialCollections = GC.CollectionCount(0);
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < ITERATIONS; i++) {
            // PERFORMANCE ISSUE: String concatenation creates many temporary objects
            string prefix = "User_";          // String allocation
            string number = i.ToString();     // String allocation  
            string suffix = "_Active";        // String allocation
            string result = prefix + number + suffix; // More string allocations
            
            strings.Add(result);
            
            // PERFORMANCE ISSUE: More string operations
            if (result.Contains("User")) {    // String operations
                string modified = result.Replace("Active", "Verified"); // Another allocation
                strings.Add(modified);
            }
            
            if (i % 10000 == 0) {
                Console.WriteLine($"String operations: {i}/{ITERATIONS}, Memory: {GC.GetTotalMemory(false):N0} bytes");
            }
        }
        
        sw.Stop();
        
        long finalMemory = GC.GetTotalMemory(false);
        int finalCollections = GC.CollectionCount(0);
        
        Console.WriteLine($"String allocation overhead completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Strings created: {strings.Count}");
        Console.WriteLine($"Memory growth: {(finalMemory - initialMemory):N0} bytes");
        Console.WriteLine($"GC collections: {finalCollections - initialCollections}");
        Console.WriteLine("Many temporary strings created allocation pressure");
    }
    
    static void Main() {
        Console.WriteLine("Starting string allocation performance demonstration...");
        Console.WriteLine("Task: String operations with excessive temporary allocations");
        Console.WriteLine("Monitor Memory Usage Tool for string allocation patterns");
        Console.WriteLine();
        
        DemonstrateStringAllocationOverhead();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check Memory profiler for:");
        Console.WriteLine("- High number of string allocations");
        Console.WriteLine("- Temporary string objects in Gen 0");
        Console.WriteLine("- GC pressure from string concatenation");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR STRING OPTIMIZATION)
================================================================================

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

class Program {
    static void DemonstrateStringBuilderOptimization() {
        Console.WriteLine("Starting StringBuilder optimization demonstration...");
        Console.WriteLine("Monitor Memory profiler - should see reduced string allocations");
        
        const int ITERATIONS = 100000;
        var strings = new List<string>();
        
        long initialMemory = GC.GetTotalMemory(false);
        int initialCollections = GC.CollectionCount(0);
        
        var sw = Stopwatch.StartNew();
        
        // CORREÇÃO: Reuse StringBuilder to reduce allocations
        var sb = new StringBuilder(64); // Pre-allocate capacity
        
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: StringBuilder avoids temporary string allocations
            sb.Clear();
            sb.Append("User_");
            sb.Append(i);
            sb.Append("_Active");
            
            string result = sb.ToString(); // Single allocation for final result
            strings.Add(result);
            
            // CORREÇÃO: More efficient string operations
            if (result.AsSpan().Contains("User".AsSpan(), StringComparison.Ordinal)) {
                sb.Clear();
                sb.Append(result.AsSpan(0, result.Length - 6)); // "Active".Length = 6
                sb.Append("Verified");
                strings.Add(sb.ToString());
            }
            
            if (i % 10000 == 0) {
                Console.WriteLine($"StringBuilder operations: {i}/{ITERATIONS}, Memory: {GC.GetTotalMemory(false):N0} bytes");
            }
        }
        
        sw.Stop();
        
        long finalMemory = GC.GetTotalMemory(false);
        int finalCollections = GC.CollectionCount(0);
        
        Console.WriteLine($"StringBuilder optimization completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Strings created: {strings.Count}");
        Console.WriteLine($"Memory growth: {(finalMemory - initialMemory):N0} bytes");
        Console.WriteLine($"GC collections: {finalCollections - initialCollections}");
        Console.WriteLine("StringBuilder significantly reduced allocations");
    }
    
    static void DemonstrateStringInterning() {
        Console.WriteLine("Starting string interning demonstration...");
        
        const int ITERATIONS = 50000;
        var strings = new List<string>();
        
        var sw = Stopwatch.StartNew();
        
        // CORREÇÃO: Use string interning for repeated values
        string[] commonPrefixes = { "Admin_", "User_", "Guest_", "System_" };
        string[] commonSuffixes = { "_Active", "_Inactive", "_Pending", "_Verified" };
        
        // Intern common strings
        for (int i = 0; i < commonPrefixes.Length; i++) {
            commonPrefixes[i] = string.Intern(commonPrefixes[i]);
        }
        for (int i = 0; i < commonSuffixes.Length; i++) {
            commonSuffixes[i] = string.Intern(commonSuffixes[i]);
        }
        
        for (int i = 0; i < ITERATIONS; i++) {
            string prefix = commonPrefixes[i % commonPrefixes.Length]; // Reused interned string
            string suffix = commonSuffixes[i % commonSuffixes.Length]; // Reused interned string
            string number = (i % 1000).ToString(); // Limited range for better interning
            
            string result = string.Intern(prefix + number + suffix); // Intern final result
            strings.Add(result);
            
            if (i % 10000 == 0) {
                Console.WriteLine($"String interning: {i}/{ITERATIONS}");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"String interning completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Unique strings due to interning: {strings.Count}");
        Console.WriteLine("String interning reduced memory usage for repeated strings");
    }
    
    static void DemonstrateSpanOptimization() {
        Console.WriteLine("Starting Span<char> optimization demonstration...");
        
        const int ITERATIONS = 100000;
        var results = new List<int>();
        
        var sw = Stopwatch.StartNew();
        
        string sourceText = "The quick brown fox jumps over the lazy dog. " +
                           "This is a sample text for demonstrating Span operations. " +
                           "Span allows us to work with string slices without allocation.";
        
        ReadOnlySpan<char> sourceSpan = sourceText.AsSpan();
        
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: Span operations avoid string allocations
            int startIndex = i % (sourceText.Length - 10);
            ReadOnlySpan<char> slice = sourceSpan.Slice(startIndex, 10); // No allocation
            
            // Work with span without creating intermediate strings
            int wordCount = 0;
            for (int j = 0; j < slice.Length; j++) {
                if (slice[j] == ' ') {
                    wordCount++;
                }
            }
            
            results.Add(wordCount);
            
            if (i % 20000 == 0) {
                Console.WriteLine($"Span operations: {i}/{ITERATIONS}");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Span<char> optimization completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Results computed: {results.Count}");
        Console.WriteLine("Span<char> operations created zero string allocations");
    }
    
    static void DemonstrateStringPooling() {
        Console.WriteLine("Starting string pooling demonstration...");
        
        const int ITERATIONS = 50000;
        
        // CORREÇÃO: String pool for commonly used strings
        var stringPool = new Dictionary<string, string>();
        var results = new List<string>();
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < ITERATIONS; i++) {
            string key = $"Item_{i % 1000}_Status"; // Limited patterns
            
            // CORREÇÃO: Pool identical strings
            if (!stringPool.TryGetValue(key, out string pooledString)) {
                pooledString = key;
                stringPool[key] = pooledString;
            }
            
            results.Add(pooledString); // Reuse pooled instance
            
            if (i % 10000 == 0) {
                Console.WriteLine($"String pooling: {i}/{ITERATIONS}, Pool size: {stringPool.Count}");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"String pooling completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Results: {results.Count}, Unique strings in pool: {stringPool.Count}");
        Console.WriteLine("String pooling maximized reuse of identical strings");
    }
    
    static void DemonstrateFormatOptimization() {
        Console.WriteLine("Starting format optimization demonstration...");
        
        const int ITERATIONS = 100000;
        var results = new List<string>();
        
        var sw = Stopwatch.StartNew();
        
        // CORREÇÃO: Reuse format strings and StringBuilder
        const string FORMAT_TEMPLATE = "User ID: {0}, Score: {1:F2}, Active: {2}";
        var sb = new StringBuilder(64);
        
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: Use StringBuilder with AppendFormat
            sb.Clear();
            sb.AppendFormat(FORMAT_TEMPLATE, i, i * 0.1, i % 2 == 0);
            
            results.Add(sb.ToString());
            
            if (i % 20000 == 0) {
                Console.WriteLine($"Format optimization: {i}/{ITERATIONS}");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Format optimization completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Formatted strings: {results.Count}");
        Console.WriteLine("StringBuilder.AppendFormat reduced intermediate allocations");
    }
    
    static void Main() {
        Console.WriteLine("Starting optimized string operations demonstration...");
        Console.WriteLine("Task: Efficient string handling with reduced allocations");
        Console.WriteLine("Monitor Memory Usage Tool for improved string performance");
        Console.WriteLine();
        
        DemonstrateStringBuilderOptimization();
        Console.WriteLine();
        DemonstrateStringInterning();
        Console.WriteLine();
        DemonstrateSpanOptimization();
        Console.WriteLine();
        DemonstrateStringPooling();
        Console.WriteLine();
        DemonstrateFormatOptimization();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("String optimization strategies:");
        Console.WriteLine("- StringBuilder eliminates intermediate string allocations");
        Console.WriteLine("- String interning reuses identical strings");
        Console.WriteLine("- Span<char> enables allocation-free string slicing");
        Console.WriteLine("- String pooling maximizes reuse patterns");
        Console.WriteLine("- Format optimization reduces temporary objects");
        Console.WriteLine("- Dramatically reduced memory pressure and GC collections");
    }
}

================================================================================
*/
