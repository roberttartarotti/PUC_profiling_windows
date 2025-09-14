/*
================================================================================
ATIVIDADE PRÁTICA 1 - PROFILING BÁSICO DE CPU COM FUNÇÃO LENTA (C#)
================================================================================

OBJETIVO:
- Criar aplicação com código que causa overhead de CPU
- Executar fora do profiler para observar tempo de execução
- Usar Performance Profiler com CPU Usage habilitado
- Identificar função que consome mais tempo de CPU
- Corrigir código removendo atraso e validar ganhos

PROBLEMA:
- Função factorial contém loop interno artificial que causa overhead
- CPU Usage Tool mostrará hotspot na função SlowFactorial()

SOLUÇÃO:
- Remover o loop artificial interno (linhas do delay)
- Resultado: dramatica redução no tempo de execução

================================================================================
*/

using System;
using System.Diagnostics;

class Program {
    static long SlowFactorial(int n) {
        long result = 1;
        for (int i = 1; i <= n; i++) {
            for (int j = 0; j < 100000; j++); // PERFORMANCE BOTTLENECK: Remove this artificial delay loop to fix CPU overhead
            result *= i;
        }
        return result;
    }

    static void Main() {
        var sw = Stopwatch.StartNew();
        Console.WriteLine("Factorial(20) = " + SlowFactorial(20));
        sw.Stop();
        Console.WriteLine("Time = " + sw.ElapsedMilliseconds + " ms");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR A VERSÃO OTIMIZADA)
================================================================================

using System;
using System.Diagnostics;

class Program {
    static long FastFactorial(int n) {
        long result = 1;
        for (int i = 1; i <= n; i++) {
            // CORREÇÃO: Removido o loop artificial que causava overhead
            result *= i;
        }
        return result;
    }

    static void Main() {
        var sw = Stopwatch.StartNew();
        Console.WriteLine("Factorial(20) = " + FastFactorial(20));
        sw.Stop();
        Console.WriteLine("Time = " + sw.ElapsedMilliseconds + " ms");
    }
}

================================================================================
*/
