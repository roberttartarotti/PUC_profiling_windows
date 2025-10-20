using System;
using System.Diagnostics.Tracing;

[EventSource(Name = "XmlStringAccumulation-EventSource")]
public sealed class XmlStringEventSource : EventSource
{
    public static XmlStringEventSource Log = new XmlStringEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static string BuildLargeXML(int items)
    {
        string xml = "<root>";
        
        for (int i = 0; i < items; i++)
        {
            xml += "<item id='";
            xml += i.ToString();
            xml += "'>";
            xml += "<name>Item";
            xml += i.ToString();
            xml += "</name>";
            xml += "<description>This is a description for item ";
            xml += i.ToString();
            xml += "</description>";
            xml += "<value>";
            xml += (i * 100).ToString();
            xml += "</value>";
            xml += "</item>";
        }
        
        xml += "</root>";
        return xml;
    }

    static void ProcessXMLGeneration()
    {
        for (int i = 0; i < 100; i++)
        {
            string result = BuildLargeXML(500);
            int len = result.Length;
        }
    }

    static void Main()
    {
        XmlStringEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 200; i++)
        {
            ProcessXMLGeneration();
        }

        XmlStringEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

