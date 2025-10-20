using System;
using System.Diagnostics.Tracing;
using System.Threading;

[EventSource(Name = "PriorityInversion-EventSource")]
public sealed class PriorityInversionEventSource : EventSource
{
    public static PriorityInversionEventSource Log = new PriorityInversionEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static object lockObject = new object();
    static bool continueRunning = true;

    static void LowPriorityTask()
    {
        Thread.CurrentThread.Priority = ThreadPriority.Lowest;
        
        for (int i = 0; i < 50; i++)
        {
            lock (lockObject)
            {
                int work = 0;
                for (int j = 0; j < 10000000; j++)
                {
                    work += j;
                }
            }
            Thread.Sleep(10);
        }
    }

    static void HighPriorityTask()
    {
        Thread.CurrentThread.Priority = ThreadPriority.Highest;
        
        Thread.Sleep(50);
        
        for (int i = 0; i < 100; i++)
        {
            lock (lockObject)
            {
                int work = 0;
                for (int j = 0; j < 100; j++)
                {
                    work += j;
                }
            }
        }
    }

    static void MediumPriorityTask()
    {
        Thread.CurrentThread.Priority = ThreadPriority.Normal;
        
        Thread.Sleep(20);
        
        int work = 0;
        for (int i = 0; i < 1000000000; i++)
        {
            work += i;
            if (!continueRunning) break;
        }
    }

    static void Main()
    {
        PriorityInversionEventSource.Log.ProcessingStarted();

        Thread lowThread = new Thread(LowPriorityTask);
        Thread medThread = new Thread(MediumPriorityTask);
        Thread highThread = new Thread(HighPriorityTask);
        
        lowThread.Start();
        medThread.Start();
        highThread.Start();
        
        lowThread.Join();
        highThread.Join();
        continueRunning = false;
        medThread.Join();

        PriorityInversionEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

