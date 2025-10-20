using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Threading;

[EventSource(Name = "BusyWaitLoop-EventSource")]
public sealed class BusyWaitEventSource : EventSource
{
    public static BusyWaitEventSource Log = new BusyWaitEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static void ProcessTask(int iterations)
    {
        int result = 0;
        var start = DateTime.UtcNow;
        var end = start.AddMilliseconds(100);
        
        while (DateTime.UtcNow < end)
        {
            result++;
        }
        
        for (int i = 0; i < iterations; i++)
        {
            result += i * i;
        }
    }

    static void Main()
    {
        BusyWaitEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 5000; i++)
        {
            ProcessTask(10000);
        }

        BusyWaitEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

