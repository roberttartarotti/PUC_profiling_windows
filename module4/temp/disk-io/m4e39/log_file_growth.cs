using System;
using System.Diagnostics.Tracing;
using System.IO;

[EventSource(Name = "LogFileGrowth-EventSource")]
public sealed class LogFileGrowthEventSource : EventSource
{
    public static LogFileGrowthEventSource Log = new LogFileGrowthEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static void WriteLogEntries()
    {
        using (StreamWriter writer = new StreamWriter("application.log", true))
        {
            for (int i = 0; i < 10000; i++)
            {
                DateTime now = DateTime.Now;
                string logEntry = $"{now:yyyy-MM-dd HH:mm:ss.fff} [INFO] Processing item {i} with extensive details and context information that makes each log entry quite large";
                
                writer.WriteLine(logEntry);
            }
        }
    }

    static void Main()
    {
        LogFileGrowthEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 100; i++)
        {
            WriteLogEntries();
        }

        File.Delete("application.log");

        LogFileGrowthEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

