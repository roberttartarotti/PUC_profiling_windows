using System;
using System.Threading;
using System.Diagnostics;

class DifferentLocksExample
{
    static void Main()
    {
        Console.WriteLine("=== EXEMPLO 4: TIPOS DE LOCKS EM C# ===\n");

        // 1. LOCK (Monitor) - O mais comum
        Console.WriteLine("1. LOCK (Monitor)");
        DemonstrateLock();

        // 2. Mutex - Entre processos
        Console.WriteLine("\n2. MUTEX (Entre processos)");
        DemonstrateMutex();

        // 3. Semáforo - Permite N threads
        Console.WriteLine("\n3. SEMÁFORO (Permite N threads simultâneas)");
        DemonstrateSemaphore();

        // 4. ReaderWriterLockSlim - Múltiplos leitores
        Console.WriteLine("\n4. READER/WRITER LOCK (Múltiplos leitores, escritor exclusivo)");
        DemonstrateReaderWriterLock();
    }

    // 1. LOCK
    static void DemonstrateLock()
    {
        object lockObj = new object();
        int value = 0;

        var watch = Stopwatch.StartNew();

        Thread[] threads = new Thread[5];
        for (int i = 0; i < 5; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 100_000; j++)
                {
                    lock (lockObj)
                    {
                        value++;
                    }
                }
            });
            threads[i].Start();
        }

        foreach (var t in threads) t.Join();

        watch.Stop();
        Console.WriteLine($"  Valor: {value}, Tempo: {watch.ElapsedMilliseconds}ms");
        Console.WriteLine($"  Uso: lock(objeto) {{ código crítico }}");
    }

    // 2. MUTEX
    static void DemonstrateMutex()
    {
        using (Mutex mutex = new Mutex())
        {
            int value = 0;

            var watch = Stopwatch.StartNew();

            Thread[] threads = new Thread[5];
            for (int i = 0; i < 5; i++)
            {
                threads[i] = new Thread(() =>
                {
                    for (int j = 0; j < 100_000; j++)
                    {
                        mutex.WaitOne();
                        try
                        {
                            value++;
                        }
                        finally
                        {
                            mutex.ReleaseMutex();
                        }
                    }
                });
                threads[i].Start();
            }

            foreach (var t in threads) t.Join();

            watch.Stop();
            Console.WriteLine($"  Valor: {value}, Tempo: {watch.ElapsedMilliseconds}ms");
            Console.WriteLine($"  Uso: mutex.WaitOne(); ... mutex.ReleaseMutex();");
        }
    }

    // 3. SEMÁFORO
    static void DemonstrateSemaphore()
    {
        using (Semaphore semaphore = new Semaphore(2, 2)) // Permite 2 threads simultâneas
        {
            int concurrent = 0;
            int maxConcurrent = 0;
            object statsLock = new object();

            Console.WriteLine("  Semáforo com capacidade para 2 threads simultâneas:");

            Thread[] threads = new Thread[5];
            for (int i = 0; i < 5; i++)
            {
                int threadId = i;
                threads[i] = new Thread(() =>
                {
                    semaphore.WaitOne();
                    try
                    {
                        lock (statsLock)
                        {
                            concurrent++;
                            if (concurrent > maxConcurrent)
                                maxConcurrent = concurrent;
                            Console.WriteLine($"    Thread {threadId} adquiriu (total: {concurrent})");
                        }

                        Thread.Sleep(100); // Simula trabalho

                        lock (statsLock)
                        {
                            concurrent--;
                            Console.WriteLine($"    Thread {threadId} liberou (total: {concurrent})");
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
                threads[i].Start();
            }

            foreach (var t in threads) t.Join();

            Console.WriteLine($"  Máximo concorrente observado: {maxConcurrent} (esperado: 2)");
        }
    }

    // 4. READER/WRITER LOCK
    static void DemonstrateReaderWriterLock()
    {
        ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
        int value = 0;

        // 8 threads: 6 leitores, 2 escritores
        Thread[] threads = new Thread[8];

        // Threads leitoras
        for (int i = 0; i < 6; i++)
        {
            int id = i;
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 1000; j++)
                {
                    rwLock.EnterReadLock();
                    try
                    {
                        int x = value; // Lê valor
                    }
                    finally
                    {
                        rwLock.ExitReadLock();
                    }
                }
            });
        }

        // Threads escritoras
        for (int i = 6; i < 8; i++)
        {
            int id = i;
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 500; j++)
                {
                    rwLock.EnterWriteLock();
                    try
                    {
                        value++; // Modifica valor
                    }
                    finally
                    {
                        rwLock.ExitWriteLock();
                    }
                }
            });
        }

        var watch = Stopwatch.StartNew();
        foreach (var t in threads) t.Start();
        foreach (var t in threads) t.Join();
        watch.Stop();

        Console.WriteLine($"  Tempo: {watch.ElapsedMilliseconds}ms");
        Console.WriteLine($"  Valor final: {value}");
    }
}
