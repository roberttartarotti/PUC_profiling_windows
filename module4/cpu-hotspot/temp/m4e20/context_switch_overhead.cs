using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;

[EventSource(Name = "ContextSwitch-EventSource")]
public sealed class ContextSwitchEventSource : EventSource
{
    public static ContextSwitchEventSource Log = new ContextSwitchEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static ManualResetEvent signal = new ManualResetEvent(false);

    static void ShortLivedThread()
    {
        int result = 0;
        
        for (int i = 0; i < 100; i++)
        {
            result += i;
        }
        
        signal.WaitOne(1);
    }

    static void CreateManyShortThreads()
    {
        List<Thread> threads = new List<Thread>();
        
        for (int i = 0; i < 1000; i++)
        {
            Thread thread = new Thread(ShortLivedThread);
            thread.Start();
            threads.Add(thread);
            
            if (i % 100 == 0)
            {
                signal.Set();
            }
        }
        
        signal.Set();
        
        foreach (Thread thread in threads)
        {
            thread.Join();
        }
    }

    static void Main()
    {
        ContextSwitchEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 50; i++)
        {
            signal.Reset();
            CreateManyShortThreads();
        }

        ContextSwitchEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

