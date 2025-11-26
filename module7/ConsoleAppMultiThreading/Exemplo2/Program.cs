// See https://aka.ms/new-console-template for more information
using System;
using System.Threading;
using System.Diagnostics;

class LockExample
{
    private static int counter = 0;
    private static readonly object lockObject = new object();  // ← LOCK OBJECT

    static void Main()
    {
        Console.WriteLine("=== EXEMPLO 2: USANDO LOCK (COM PROTEÇÃO) ===\n");

        counter = 0;
        var watch = Stopwatch.StartNew();

        Thread[] threads = new Thread[10];
        for (int i = 0; i < 10; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 1_000_000; j++)
                {
                    lock (lockObject)  // ← SEÇÃO CRÍTICA PROTEGIDA
                    {
                        counter++;
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

        Console.WriteLine($"Valor final: {counter}");
        Console.WriteLine($"Tempo: {watch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Resultado correto! ✓\n");

        Console.WriteLine("EXPLICAÇÃO:");
        Console.WriteLine("O 'lock' garante que apenas UMA thread execute");
        Console.WriteLine("o código dentro do bloco por vez.");
        Console.WriteLine("\nMANTER LOCK O MÍNIMO POSSÍVEL!");
        Console.WriteLine("Locks longos causam contenção - threads aguardando.");
    }
}
