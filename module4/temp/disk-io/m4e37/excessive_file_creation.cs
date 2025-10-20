using System;
using System.Diagnostics.Tracing;
using System.IO;

[EventSource(Name = "ExcessiveFileCreation-EventSource")]
public sealed class FileCreationEventSource : EventSource
{
    public static FileCreationEventSource Log = new FileCreationEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static void CreateDeleteFiles()
    {
        for (int i = 0; i < 5000; i++)
        {
            string filename = $"temp_{i}.tmp";
            
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                byte[] data = new byte[256];
                
                for (int j = 0; j < 256; j++)
                {
                    data[j] = (byte)(j % 256);
                }
                
                fs.Write(data, 0, 256);
            }
            
            File.Delete(filename);
        }
    }

    static void Main()
    {
        FileCreationEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 50; i++)
        {
            CreateDeleteFiles();
        }

        FileCreationEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

