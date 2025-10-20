using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

[EventSource(Name = "StaticCollectionGrowth-EventSource")]
public sealed class StaticCollectionGrowthEventSource : EventSource
{
    public static StaticCollectionGrowthEventSource Log = new StaticCollectionGrowthEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class CacheEntry
{
    public int Id { get; set; }
    public byte[] Data { get; set; }
    
    public CacheEntry(int id)
    {
        Id = id;
        Data = new byte[50000];
        for (int i = 0; i < Data.Length; i += 100)
        {
            Data[i] = (byte)(i % 256);
        }
    }
}

class Program
{
    static Dictionary<int, CacheEntry> globalCache = new Dictionary<int, CacheEntry>();

    static void AddToCache(int id)
    {
        if (!globalCache.ContainsKey(id))
        {
            globalCache[id] = new CacheEntry(id);
        }
        
        int sum = 0;
        foreach (var entry in globalCache.Values)
        {
            sum += entry.Data[0];
        }
    }

    static void Main()
    {
        StaticCollectionGrowthEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 10000; i++)
        {
            AddToCache(i);
        }

        StaticCollectionGrowthEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

