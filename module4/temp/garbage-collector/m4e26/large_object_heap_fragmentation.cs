using System;
using System.Diagnostics.Tracing;

[EventSource(Name = "LOHFragmentation-EventSource")]
public sealed class LOHFragmentationEventSource : EventSource
{
    public static LOHFragmentationEventSource Log = new LOHFragmentationEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static void AllocateLargeObjects()
    {
        byte[] temp1 = new byte[90000];
        byte[] temp2 = new byte[100000];
        byte[] temp3 = new byte[85000];
        byte[] temp4 = new byte[95000];
        byte[] temp5 = new byte[88000];
        
        for (int i = 0; i < temp1.Length; i += 1000)
        {
            temp1[i] = (byte)(i % 256);
        }
        
        for (int i = 0; i < temp2.Length; i += 1000)
        {
            temp2[i] = (byte)(i % 256);
        }
        
        for (int i = 0; i < temp3.Length; i += 1000)
        {
            temp3[i] = (byte)(i % 256);
        }
        
        for (int i = 0; i < temp4.Length; i += 1000)
        {
            temp4[i] = (byte)(i % 256);
        }
        
        for (int i = 0; i < temp5.Length; i += 1000)
        {
            temp5[i] = (byte)(i % 256);
        }
    }

    static void Main()
    {
        LOHFragmentationEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 10000; i++)
        {
            AllocateLargeObjects();
        }

        LOHFragmentationEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

