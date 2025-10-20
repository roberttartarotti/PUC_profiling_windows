using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

[EventSource(Name = "Gen2Pressure-EventSource")]
public sealed class Gen2PressureEventSource : EventSource
{
    public static Gen2PressureEventSource Log = new Gen2PressureEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class LongLivedData
{
    public byte[] data;
    public List<int> references;
    
    public LongLivedData(int size)
    {
        data = new byte[size];
        references = new List<int>();
        
        for (int i = 0; i < size; i++)
        {
            data[i] = (byte)(i % 256);
        }
        
        for (int i = 0; i < 1000; i++)
        {
            references.Add(i);
        }
    }
}

class Program
{
    static List<LongLivedData> persistentData = new List<LongLivedData>();

    static void AllocateData()
    {
        for (int i = 0; i < 100; i++)
        {
            LongLivedData data = new LongLivedData(50000);
            persistentData.Add(data);
            
            byte[] tempData = new byte[30000];
            for (int j = 0; j < tempData.Length; j += 100)
            {
                tempData[j] = (byte)(j % 256);
            }
        }
        
        if (persistentData.Count > 5000)
        {
            persistentData.RemoveRange(0, 1000);
        }
    }

    static void Main()
    {
        Gen2PressureEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 200; i++)
        {
            AllocateData();
        }

        Gen2PressureEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

