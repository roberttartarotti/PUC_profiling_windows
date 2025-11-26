using System;
using System.Threading;

class DeadlockExample
{
    // Dois locks separados
    private static readonly object lock1 = new object();
    private static readonly object lock2 = new object();

    static void Main()
    {
        Console.WriteLine("=== EXEMPLO 5: DEADLOCK ===\n");

        Console.WriteLine("Iniciando deadlock em 3 segundos...");
        Console.WriteLine("(O programa vai travar - você terá que fechar)\n");

        Thread thread1 = new Thread(Thread1Func);
        Thread thread2 = new Thread(Thread2Func);

        thread1.Start();
        thread2.Start();

        // Esperar para ver o deadlock ocorrer
        Thread.Sleep(2000);

        if (thread1.IsAlive && thread2.IsAlive)
        {
            Console.WriteLine("❌ DEADLOCK DETECTADO!");
            Console.WriteLine("\nO QUE ACONTECEU:");
            Console.WriteLine("Thread 1: Adquiriu lock1, aguardando lock2");
            Console.WriteLine("Thread 2: Adquiriu lock2, aguardando lock1");
            Console.WriteLine("\nAmbas aguardando para sempre!");
            Console.WriteLine("\nPressione Ctrl+C para sair...");

            // Tentar terminar threads
            thread1.Join(TimeSpan.FromSeconds(1));
            thread2.Join(TimeSpan.FromSeconds(1));
        }
    }

    static void Thread1Func()
    {
        lock (lock1)
        {
            Console.WriteLine("Thread 1: Adquiriu lock1");
            Thread.Sleep(100); // Dar tempo para thread 2 adquirir lock2

            Console.WriteLine("Thread 1: Tentando adquirir lock2...");
            lock (lock2)  // ← DEADLOCK: Aguardará para sempre
            {
                Console.WriteLine("Thread 1: Adquiriu lock2 (você nunca verá isto!)");
            }
        }
    }

    static void Thread2Func()
    {
        lock (lock2)
        {
            Console.WriteLine("Thread 2: Adquiriu lock2");
            Thread.Sleep(100);

            Console.WriteLine("Thread 2: Tentando adquirir lock1...");
            lock (lock1)  // ← DEADLOCK: Aguardará para sempre
            {
                Console.WriteLine("Thread 2: Adquiriu lock1 (você nunca verá isto!)");
            }
        }
    }
}
