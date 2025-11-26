using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

class ContentionExample
{
    private static int sharedValue = 0;
    private static readonly object lockObject = new object();

    static void Main()
    {
        Console.WriteLine("=== EXEMPLO 3: CONTENÇÃO DE THREADS ===\n");

        // Teste com 2 threads
        Console.WriteLine("Teste 1: 2 threads competindo pelo lock");
        MeasureContention(2);

        // Teste com 4 threads
        Console.WriteLine("\nTeste 2: 4 threads competindo pelo lock");
        MeasureContention(4);

        // Teste com 8 threads (saturação)
        Console.WriteLine("\nTeste 3: 8 threads competindo pelo lock");
        MeasureContention(8);

    }

    static void MeasureContention(int numThreads)
    {
        sharedValue = 0;
        var watch = Stopwatch.StartNew();

        Thread[] threads = new Thread[numThreads];
        for (int i = 0; i < numThreads; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 10_000_000; j++)
                {
                    lock (lockObject)
                    {
                        sharedValue++;
                    }
                }
            });
            threads[i].Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        watch.Stop();

        Console.WriteLine($"  {numThreads} threads: {watch.ElapsedMilliseconds}ms");
        Console.WriteLine($"  Ops/segundo: {(numThreads * 10_000_000.0 / watch.ElapsedMilliseconds * 1000):F0}");
    }
}
