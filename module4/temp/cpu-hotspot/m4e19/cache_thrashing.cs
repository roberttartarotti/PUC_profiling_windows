using System;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;

[EventSource(Name = "CacheThrashing-EventSource")]
public sealed class CacheThrashingEventSource : EventSource
{
    public static CacheThrashingEventSource Log = new CacheThrashingEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static int[] globalCounters = new int[8];

    static void IncrementCounter(int threadId)
    {
        for (int i = 0; i < 10000000; i++)
        {
            Interlocked.Increment(ref globalCounters[threadId % 8]);
        }
    }

    static void RunThreads()
    {
        Task[] tasks = new Task[8];
        
        for (int i = 0; i < 8; i++)
        {
            int threadId = i;
            tasks[i] = Task.Run(() => IncrementCounter(threadId));
        }
        
        Task.WaitAll(tasks);
    }

    static void Main()
    {
        CacheThrashingEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 8; i++)
        {
            globalCounters[i] = 0;
        }

        RunThreads();

        CacheThrashingEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

