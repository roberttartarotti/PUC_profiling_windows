using System;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;

[EventSource(Name = "SpinLockContention-EventSource")]
public sealed class SpinLockEventSource : EventSource
{
    public static SpinLockEventSource Log = new SpinLockEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static SpinLock spinLock = new SpinLock(false);
    static int sharedCounter = 0;

    static void WorkerFunction(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            bool lockTaken = false;
            try
            {
                spinLock.Enter(ref lockTaken);
                
                int temp = 0;
                for (int j = 0; j < 1000; j++)
                {
                    temp += j;
                }
                sharedCounter++;
            }
            finally
            {
                if (lockTaken) spinLock.Exit();
            }
        }
    }

    static void RunContentionTest()
    {
        Task[] tasks = new Task[16];
        
        for (int i = 0; i < 16; i++)
        {
            tasks[i] = Task.Run(() => WorkerFunction(1000));
        }
        
        Task.WaitAll(tasks);
    }

    static void Main()
    {
        SpinLockEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 20; i++)
        {
            sharedCounter = 0;
            RunContentionTest();
        }

        SpinLockEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

