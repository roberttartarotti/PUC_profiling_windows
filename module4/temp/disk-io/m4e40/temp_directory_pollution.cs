using System;
using System.Diagnostics.Tracing;
using System.IO;

[EventSource(Name = "TempDirectoryPollution-EventSource")]
public sealed class TempPollutionEventSource : EventSource
{
    public static TempPollutionEventSource Log = new TempPollutionEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static void CreateTempFiles()
    {
        string tempPath = Path.GetTempPath();
        
        for (int i = 0; i < 1000; i++)
        {
            string filename = Path.Combine(tempPath, $"tempdata_{i}_{Environment.TickCount}.tmp");
            
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.None))
            {
                byte[] data = new byte[4096];
                
                for (int j = 0; j < 4096; j++)
                {
                    data[j] = (byte)(j % 256);
                }
                
                for (int k = 0; k < 10; k++)
                {
                    fs.Write(data, 0, 4096);
                }
            }
        }
    }

    static void Main()
    {
        TempPollutionEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 20; i++)
        {
            CreateTempFiles();
        }

        TempPollutionEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

