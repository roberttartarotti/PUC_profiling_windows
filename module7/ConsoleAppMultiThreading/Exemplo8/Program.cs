using System;
using System.Threading;
using System.Diagnostics;

class CriticalSectionOptimizationExample
{
    private static readonly object lockObject = new object();
    private static int value = 0;

    static void Main()
    {
        Console.WriteLine("=== EXEMPLO 8: SEÇÕES CRÍTICAS CURTAS ===\n");

        Console.WriteLine("Lock longo com processamento pesado");
        MeasureBadApproach();

        Console.WriteLine("\nLock curto, só para dados críticos");
        MeasureGoodApproach();
    }

    // ❌ MÁ PRÁTICA: Lock contém operação pesada
    static void MeasureBadApproach()
    {
        value = 0;
        var watch = Stopwatch.StartNew();

        Thread[] threads = new Thread[8];
        for (int i = 0; i < 8; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 1_000_000; j++)
                {
                    lock (lockObject)
                    {
                        value++;
                        // ❌ OPERAÇÃO PESADA DENTRO DO LOCK!
                        for (int k = 0; k < 1_000; k++)
                        {
                            Math.Sqrt(k);
                        }
                    }
                }
            });
            threads[i].Start();
        }

        foreach (var t in threads) t.Join();

        watch.Stop();
        Console.WriteLine($"  Tempo: {watch.ElapsedMilliseconds}ms");
    }

    // ✓ BOA PRÁTICA: Lock mínimo
    static void MeasureGoodApproach()
    {
        value = 0;
        var watch = Stopwatch.StartNew();

        Thread[] threads = new Thread[8];
        for (int i = 0; i < 8; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 1_000_000; j++)
                {
                    // ✓ OPERAÇÃO PESADA FORA DO LOCK
                    double result = 0;
                    for (int k = 0; k < 1_000; k++)
                    {
                        result = Math.Sqrt(k);
                    }

                    Interlocked.Add(ref value, 1); // Simula uso do resultado para evitar otimização]
                }
            });
            threads[i].Start();
        }

        foreach (var t in threads) t.Join();

        watch.Stop();
        Console.WriteLine($"  Tempo: {watch.ElapsedMilliseconds}ms");
    }
}
