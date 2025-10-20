using System;
using System.Diagnostics.Tracing;
using System.IO;

[EventSource(Name = "UnbufferedSmallWrites-EventSource")]
public sealed class UnbufferedWritesEventSource : EventSource
{
    public static UnbufferedWritesEventSource Log = new UnbufferedWritesEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static void WriteSmallChunks()
    {
        using (FileStream fs = new FileStream("output_data.bin", FileMode.Create, FileAccess.Write, FileShare.None, 1, FileOptions.WriteThrough))
        {
            byte[] buffer = new byte[16];
            
            for (int i = 0; i < 50000; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    buffer[j] = (byte)(i % 256);
                }
                
                fs.Write(buffer, 0, 16);
                fs.Flush(true);
            }
        }
    }

    static void Main()
    {
        UnbufferedWritesEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 20; i++)
        {
            WriteSmallChunks();
            File.Delete("output_data.bin");
        }

        UnbufferedWritesEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

