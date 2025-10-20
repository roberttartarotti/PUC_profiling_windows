using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

[EventSource(Name = "ClosureAllocations-EventSource")]
public sealed class ClosureAllocationsEventSource : EventSource
{
    public static ClosureAllocationsEventSource Log = new ClosureAllocationsEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static void ProcessWithClosures()
    {
        List<int> numbers = new List<int>();
        for (int i = 0; i < 5000; i++)
        {
            numbers.Add(i);
        }
        
        for (int multiplier = 1; multiplier <= 10; multiplier++)
        {
            var results = numbers.Where(n => n % multiplier == 0)
                                .Select(n => n * multiplier)
                                .ToList();
            
            int sum = results.Sum();
        }
    }

    static void Main()
    {
        ClosureAllocationsEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 1000; i++)
        {
            ProcessWithClosures();
        }

        ClosureAllocationsEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

