using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;

[EventSource(Name = "ExcessiveThreadCreation-EventSource")]
public sealed class ThreadCreationEventSource : EventSource
{
    public static ThreadCreationEventSource Log = new ThreadCreationEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static void WorkerThread(object data)
    {
        int value = (int)data;
        int result = 0;
        
        for (int i = 0; i < 50000; i++)
        {
            result += value * i;
        }
    }

    static void ProcessData(int value)
    {
        List<Thread> threads = new List<Thread>();
        
        for (int i = 0; i < 500; i++)
        {
            Thread thread = new Thread(WorkerThread);
            thread.Start(value + i);
            threads.Add(thread);
        }
        
        foreach (Thread thread in threads)
        {
            thread.Join();
        }
    }

    static void Main()
    {
        ThreadCreationEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 50; i++)
        {
            ProcessData(i);
        }

        ThreadCreationEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

