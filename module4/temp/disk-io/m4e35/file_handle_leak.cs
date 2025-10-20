using System;
using System.Diagnostics.Tracing;
using System.IO;

[EventSource(Name = "FileHandleLeak-EventSource")]
public sealed class FileHandleLeakEventSource : EventSource
{
    public static FileHandleLeakEventSource Log = new FileHandleLeakEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static void CreateAndWriteFiles()
    {
        for (int i = 0; i < 500; i++)
        {
            string filename = $"tempfile_{i}.dat";
            
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            
            byte[] buffer = new byte[1024];
            for (int j = 0; j < 1024; j++)
            {
                buffer[j] = (byte)(j % 256);
            }
            
            for (int k = 0; k < 100; k++)
            {
                fs.Write(buffer, 0, 1024);
            }
        }
    }

    static void CleanupFiles()
    {
        for (int i = 0; i < 500; i++)
        {
            string filename = $"tempfile_{i}.dat";
            try
            {
                File.Delete(filename);
            }
            catch { }
        }
    }

    static void Main()
    {
        FileHandleLeakEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 10; i++)
        {
            CreateAndWriteFiles();
            CleanupFiles();
        }

        FileHandleLeakEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

