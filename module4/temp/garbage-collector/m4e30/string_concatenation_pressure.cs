using System;
using System.Diagnostics.Tracing;

[EventSource(Name = "StringConcatenation-EventSource")]
public sealed class StringConcatenationEventSource : EventSource
{
    public static StringConcatenationEventSource Log = new StringConcatenationEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static string BuildLargeString(int iterations)
    {
        string result = "";
        
        for (int i = 0; i < iterations; i++)
        {
            result += "Item " + i.ToString() + " - ";
            result += "Value: " + (i * 100).ToString() + ", ";
            result += "Status: Active, ";
            result += "Description: This is a longer description for item " + i.ToString() + "; ";
        }
        
        return result;
    }

    static void ProcessStrings()
    {
        for (int i = 0; i < 100; i++)
        {
            string data = BuildLargeString(500);
            int length = data.Length;
        }
    }

    static void Main()
    {
        StringConcatenationEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 200; i++)
        {
            ProcessStrings();
        }

        StringConcatenationEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

