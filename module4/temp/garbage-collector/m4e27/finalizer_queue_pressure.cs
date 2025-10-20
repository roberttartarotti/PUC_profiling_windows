using System;
using System.Diagnostics.Tracing;
using System.Threading;

[EventSource(Name = "FinalizerQueuePressure-EventSource")]
public sealed class FinalizerEventSource : EventSource
{
    public static FinalizerEventSource Log = new FinalizerEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class ResourceHolder
{
    private byte[] data;
    
    public ResourceHolder(int size)
    {
        data = new byte[size];
        for (int i = 0; i < size; i += 100)
        {
            data[i] = (byte)(i % 256);
        }
    }
    
    ~ResourceHolder()
    {
        Thread.Sleep(5);
        
        if (data != null)
        {
            int sum = 0;
            for (int i = 0; i < data.Length; i += 100)
            {
                sum += data[i];
            }
        }
    }
}

class Program
{
    static void CreateObjects()
    {
        for (int i = 0; i < 1000; i++)
        {
            ResourceHolder holder = new ResourceHolder(50000);
        }
    }

    static void Main()
    {
        FinalizerEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 100; i++)
        {
            CreateObjects();
        }

        FinalizerEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

