/*
================================================================================
ATIVIDADE PRÁTICA 11 - REGEX COMPILATION PERFORMANCE (C#)
================================================================================

OBJETIVO:
- Demonstrar overhead de recompilar regex patterns repetidamente
- Usar CPU profiler para identificar tempo gasto em regex compilation
- Otimizar usando RegexOptions.Compiled e static regex
- Medir impacto da compilation vs match performance

PROBLEMA:
- Recompilar Regex patterns em loops é extremamente custoso
- new Regex() constructor realiza compilation a cada call
- CPU Profiler mostrará tempo gasto em regex compilation

SOLUÇÃO:
- Compilar regex uma vez com RegexOptions.Compiled
- Usar static readonly Regex para reutilização

================================================================================
*/

using System;
using System.Text.RegularExpressions;
using System.Diagnostics;

class Program {
    static void DemonstrateRegexRecompilation() {
        Console.WriteLine("Starting regex recompilation demonstration...");
        Console.WriteLine("Monitor CPU profiler - should see time spent in regex compilation");
        
        const int MATCH_COUNT = 10000;
        string[] testStrings = {
            "user@example.com",
            "invalid.email",
            "test@domain.org", 
            "notanemail",
            "another@test.com"
        };
        
        var sw = Stopwatch.StartNew();
        
        int validEmails = 0;
        for (int i = 0; i < MATCH_COUNT; i++) {
            string testString = testStrings[i % testStrings.Length];
            
            // PERFORMANCE ISSUE: Creating new Regex instance every iteration
            var emailPattern = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
            
            if (emailPattern.IsMatch(testString)) {
                validEmails++;
            }
            
            if (i % 1000 == 0) {
                Console.WriteLine($"Completed {i}/{MATCH_COUNT} regex compilations...");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Regex recompilation completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Valid emails found: {validEmails}/{MATCH_COUNT}");
        Console.WriteLine($"Regex compilations performed: {MATCH_COUNT}");
    }
    
    static void Main() {
        Console.WriteLine("Starting regex performance demonstration...");
        Console.WriteLine("Task: Validating email addresses with regex patterns");
        Console.WriteLine("Monitor CPU Usage Tool for regex compilation overhead");
        Console.WriteLine();
        
        DemonstrateRegexRecompilation();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check CPU profiler for:");
        Console.WriteLine("- Time spent in Regex constructor");
        Console.WriteLine("- Pattern compilation overhead");
        Console.WriteLine("- Repeated expensive compilation operations");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR A VERSÃO OTIMIZADA)
================================================================================

using System;
using System.Text.RegularExpressions;
using System.Diagnostics;

class Program {
    // CORREÇÃO: Precompiled static regex with Compiled option for best performance
    private static readonly Regex EmailPattern = new Regex(
        @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );
    
    static void DemonstratePrecompiledRegex() {
        Console.WriteLine("Starting precompiled regex demonstration...");
        Console.WriteLine("Monitor CPU profiler - should see reduced compilation overhead");
        
        const int MATCH_COUNT = 10000;
        string[] testStrings = {
            "user@example.com",
            "invalid.email",
            "test@domain.org",
            "notanemail", 
            "another@test.com"
        };
        
        var sw = Stopwatch.StartNew();
        
        int validEmails = 0;
        for (int i = 0; i < MATCH_COUNT; i++) {
            string testString = testStrings[i % testStrings.Length];
            
            // CORREÇÃO: Use precompiled static regex - no compilation overhead
            if (EmailPattern.IsMatch(testString)) {
                validEmails++;
            }
            
            if (i % 1000 == 0) {
                Console.WriteLine($"Completed {i}/{MATCH_COUNT} regex matches...");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Precompiled regex completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Valid emails found: {validEmails}/{MATCH_COUNT}");
        Console.WriteLine($"Regex compilations performed: 1 (reused {MATCH_COUNT} times)");
    }
    
    static void Main() {
        Console.WriteLine("Starting optimized regex demonstration...");
        Console.WriteLine("Task: Validating emails with precompiled regex");
        Console.WriteLine("Monitor CPU Usage Tool for improved performance");
        Console.WriteLine();
        
        DemonstratePrecompiledRegex();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("Improvements:");
        Console.WriteLine("- Single regex compilation with RegexOptions.Compiled");
        Console.WriteLine("- Dramatically reduced CPU usage");
        Console.WriteLine("- Focus on actual pattern matching vs compilation");
        Console.WriteLine("- Better scalability for high-volume matching");
        Console.WriteLine("- Static readonly ensures thread-safe reuse");
    }
}

================================================================================
*/
