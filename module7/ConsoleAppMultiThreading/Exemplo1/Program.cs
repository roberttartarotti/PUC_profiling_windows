using System;
using System.Threading;
using System.Diagnostics;

class RaceConditionExample
{
    // Sem lock - demonstra o problema
    private static int counter = 0;

    static void Main()
    {
        Console.WriteLine("=== EXEMPLO 1: RACE CONDITION (SEM PROTEÇÃO) ===\n");

        counter = 0;
        var watch = Stopwatch.StartNew();

        // Criar 10 threads, cada uma incrementa counter 1.000.000 vezes
        Thread[] threads = new Thread[10];
        for (int i = 0; i < 10; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 1_000_000; j++)
                {
                    counter++;  // ← RACE CONDITION AQUI!
                }
            });
            threads[i].Start();
        }

        // Aguardar todas as threads
        foreach (var thread in threads)
        {
            thread.Join();
        }

        watch.Stop();

        // Resultado deveria ser 10.000.000
        Console.WriteLine($"Valor final: {counter}");
        Console.WriteLine($"Tempo: {watch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Perda de dados: {10_000_000 - counter} incrementos perdidos!\n");

    }
}
