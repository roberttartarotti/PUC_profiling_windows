using System;
using System.Threading;

class TimeoutLockExample
{
    private static readonly object lock1 = new object();
    private static readonly object lock2 = new object();
    private static readonly Random random = new Random();

    static void Main()
    {
        Console.WriteLine("=== EXEMPLO 7: TIMEOUT PARA EVITAR DEADLOCK ===\n");

        Thread thread1 = new Thread(() => ThreadFunc("Thread 1", lock1, lock2));
        Thread thread2 = new Thread(() => ThreadFunc("Thread 2", lock2, lock1));

        thread1.Start();
        thread2.Start();

        thread1.Join();
        thread2.Join();

    }

    static void ThreadFunc(string threadName, object firstLock, object secondLock)
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            // Randomização para reduzir contenção sincronizada
            int randomDelay = random.Next(10, 50);
            Thread.Sleep(randomDelay);

            if (Monitor.TryEnter(firstLock, 2000))  // Timeout maior
            {
                try
                {
                    Console.WriteLine($"{threadName}: Adquiriu primeiro lock");
                    
                    // Delay menor para reduzir janela de contenção
                    Thread.Sleep(25);

                    if (Monitor.TryEnter(secondLock, 1500))
                    {
                        try
                        {
                            Console.WriteLine($"{threadName}: SUCESSO - Adquiriu ambos os locks! ✓");
                            Thread.Sleep(50); // Simula trabalho
                            return; // Sai da função com sucesso
                        }
                        finally
                        {
                            Monitor.Exit(secondLock);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{threadName}: Timeout no segundo lock - tentativa {attempt + 1}");
                    }
                }
                finally
                {
                    Monitor.Exit(firstLock);
                }
            }
            else
            {
                Console.WriteLine($"{threadName}: Timeout no primeiro lock - tentativa {attempt + 1}");
            }

            // Backoff exponencial para reduzir contenção
            if (attempt < 4)
            {
                int backoffDelay = (attempt + 1) * 100 + random.Next(50);
                Thread.Sleep(backoffDelay);
            }
        }
        
        Console.WriteLine($"{threadName}: Não conseguiu adquirir ambos os locks após 5 tentativas");
    }
}
