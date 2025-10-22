using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Threading;

[EventSource(Name = "BusyWaitLoop-EventSource", Guid = "B2345678-1234-1234-1234-123456789ABC")]
public sealed class BusyWaitEventSource : EventSource
{
    public static BusyWaitEventSource Log = new BusyWaitEventSource();

    [Event(1, Level = EventLevel.Informational, Message = "Busy wait processing started")]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational, Message = "Busy wait processing completed")]
    public void ProcessingCompleted() => WriteEvent(2);

    [Event(3, Level = EventLevel.Informational, Message = "Busy wait loop iteration {0}")]
    public void BusyWaitIteration(int iteration) => WriteEvent(3, iteration);

    [Event(4, Level = EventLevel.Informational, Message = "Task {0} completed with {1} iterations")]
    public void TaskCompleted(int taskId, int iterations) => WriteEvent(4, taskId, iterations);
}

class Program
{
    static void ProcessTask(int taskId, int iterations)
    {
        int result = 0;
        var start = DateTime.UtcNow;
        var end = start.AddMilliseconds(100);
        
        int busyWaitCount = 0;
        while (DateTime.UtcNow < end)
        {
            result++;
            busyWaitCount++;
            
            if (busyWaitCount % 10000 == 0)
            {
                BusyWaitEventSource.Log.BusyWaitIteration(busyWaitCount);
            }
        }
        
        for (int i = 0; i < iterations; i++)
        {
            result += i * i;
        }
        
        BusyWaitEventSource.Log.TaskCompleted(taskId, busyWaitCount);
    }

    static void Main()
    {
        BusyWaitEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 50; i++)
        {
            ProcessTask(i, 1000);
        }

        BusyWaitEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

