using System;
using System.Threading;

class PreventDeadlockExample
{
    private static readonly object lock1 = new object();
    private static readonly object lock2 = new object();

    static void Main()
    {
        Console.WriteLine("=== EXEMPLO 6: PREVENINDO DEADLOCK (Ordem Global) ===\n");

        Console.WriteLine("Executando com ordem consistente de locks...\n");

        Thread thread1 = new Thread(Thread1Func);
        Thread thread2 = new Thread(Thread2Func);

        thread1.Start();
        thread2.Start();

        thread1.Join();
        thread2.Join();

        Console.WriteLine("\n✓ Nenhum deadlock! Completado com sucesso!");

    }

    // Ordem: lock1 → lock2
    static void Thread1Func()
    {
        lock (lock1)
        {
            Console.WriteLine("Thread 1: Adquiriu lock1");
            Thread.Sleep(50);

            lock (lock2)
            {
                Console.WriteLine("Thread 1: Adquiriu lock2 ✓");
                Thread.Sleep(50);
            }
        }
        Console.WriteLine("Thread 1: Completada");
    }

    // Mesma ordem: lock1 → lock2 (NÃO lock2 → lock1)
    static void Thread2Func()
    {
        lock (lock1)  // ← IMPORTANTE: Mesma ordem!
        {
            Console.WriteLine("Thread 2: Adquiriu lock1");
            Thread.Sleep(50);

            lock (lock2)  // ← IMPORTANTE: Depois lock2
            {
                Console.WriteLine("Thread 2: Adquiriu lock2 ✓");
                Thread.Sleep(50);
            }
        }
        Console.WriteLine("Thread 2: Completada");
    }
}
