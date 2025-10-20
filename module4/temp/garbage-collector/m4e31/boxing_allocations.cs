using System;
using System.Collections;
using System.Diagnostics.Tracing;

[EventSource(Name = "BoxingAllocations-EventSource")]
public sealed class BoxingAllocationsEventSource : EventSource
{
    public static BoxingAllocationsEventSource Log = new BoxingAllocationsEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static void ProcessWithBoxing()
    {
        ArrayList list = new ArrayList();
        
        for (int i = 0; i < 10000; i++)
        {
            list.Add(i);
            list.Add(i * 2);
            list.Add(i * 3);
        }
        
        int sum = 0;
        foreach (object obj in list)
        {
            sum += (int)obj;
        }
        
        list.Clear();
    }

    static void Main()
    {
        BoxingAllocationsEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 5000; i++)
        {
            ProcessWithBoxing();
        }

        BoxingAllocationsEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

