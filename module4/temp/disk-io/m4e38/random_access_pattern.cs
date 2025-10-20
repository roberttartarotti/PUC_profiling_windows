using System;
using System.Diagnostics.Tracing;
using System.IO;

[EventSource(Name = "RandomAccessPattern-EventSource")]
public sealed class RandomAccessEventSource : EventSource
{
    public static RandomAccessEventSource Log = new RandomAccessEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static Random random = new Random();

    static void RandomAccessWrites()
    {
        using (FileStream fs = new FileStream("random_access.dat", FileMode.Create, FileAccess.Write))
        {
            fs.SetLength(100 * 1024 * 1024);
            
            byte[] buffer = new byte[512];
            for (int i = 0; i < 512; i++)
            {
                buffer[i] = (byte)(i % 256);
            }
            
            for (int i = 0; i < 50000; i++)
            {
                long offset = random.Next(100 * 1024 * 2) * 512L;
                
                fs.Seek(offset, SeekOrigin.Begin);
                fs.Write(buffer, 0, 512);
            }
        }
        
        File.Delete("random_access.dat");
    }

    static void Main()
    {
        RandomAccessEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 10; i++)
        {
            RandomAccessWrites();
        }

        RandomAccessEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

