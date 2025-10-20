using System;
using System.Diagnostics.Tracing;
using System.IO;
using System.Threading.Tasks;

[EventSource(Name = "SynchronousWriteBlocking-EventSource")]
public sealed class SyncWriteEventSource : EventSource
{
    public static SyncWriteEventSource Log = new SyncWriteEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static void WriteLargeFile(int threadId)
    {
        string filename = $"largefile_{threadId}.dat";
        
        using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
        {
            byte[] buffer = new byte[65536];
            
            for (int i = 0; i < 65536; i++)
            {
                buffer[i] = (byte)(i % 256);
            }
            
            for (int i = 0; i < 200; i++)
            {
                fs.Write(buffer, 0, 65536);
            }
        }
        
        File.Delete(filename);
    }

    static void RunThreadedWrites()
    {
        Task[] tasks = new Task[8];
        
        for (int i = 0; i < 8; i++)
        {
            int threadId = i;
            tasks[i] = Task.Run(() => WriteLargeFile(threadId));
        }
        
        Task.WaitAll(tasks);
    }

    static void Main()
    {
        SyncWriteEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 10; i++)
        {
            RunThreadedWrites();
        }

        SyncWriteEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

