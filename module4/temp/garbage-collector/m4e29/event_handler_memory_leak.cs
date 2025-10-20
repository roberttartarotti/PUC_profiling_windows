using System;
using System.Diagnostics.Tracing;

[EventSource(Name = "EventHandlerLeak-EventSource")]
public sealed class EventHandlerLeakEventSource : EventSource
{
    public static EventHandlerLeakEventSource Log = new EventHandlerLeakEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Publisher
{
    public event EventHandler DataChanged;
    
    public void RaiseEvent()
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }
}

class Subscriber
{
    private byte[] data;
    
    public Subscriber()
    {
        data = new byte[100000];
        for (int i = 0; i < data.Length; i += 1000)
        {
            data[i] = (byte)(i % 256);
        }
    }
    
    public void HandleDataChanged(object sender, EventArgs e)
    {
        int sum = 0;
        for (int i = 0; i < data.Length; i += 1000)
        {
            sum += data[i];
        }
    }
}

class Program
{
    static Publisher publisher = new Publisher();

    static void CreateSubscribers()
    {
        for (int i = 0; i < 100; i++)
        {
            Subscriber subscriber = new Subscriber();
            publisher.DataChanged += subscriber.HandleDataChanged;
        }
        
        publisher.RaiseEvent();
    }

    static void Main()
    {
        EventHandlerLeakEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 1000; i++)
        {
            CreateSubscribers();
        }

        EventHandlerLeakEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

