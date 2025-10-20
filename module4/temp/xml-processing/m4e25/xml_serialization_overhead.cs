using System;
using System.Diagnostics.Tracing;
using System.Xml;

[EventSource(Name = "XmlSerialization-EventSource")]
public sealed class XmlSerializationEventSource : EventSource
{
    public static XmlSerializationEventSource Log = new XmlSerializationEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static void SerializeToXML()
    {
        XmlDocument doc = new XmlDocument();
        XmlElement root = doc.CreateElement("data");
        doc.AppendChild(root);
        
        for (int i = 0; i < 200; i++)
        {
            XmlElement item = doc.CreateElement("item");
            item.SetAttribute("id", i.ToString());
            
            XmlElement name = doc.CreateElement("name");
            name.InnerText = $"Item{i}";
            item.AppendChild(name);
            
            XmlElement description = doc.CreateElement("description");
            description.InnerText = $"Content for item {i} with some additional data to make it larger";
            item.AppendChild(description);
            
            XmlElement value = doc.CreateElement("value");
            value.InnerText = (i * 100).ToString();
            item.AppendChild(value);
            
            root.AppendChild(item);
        }
        
        for (int i = 0; i < 50; i++)
        {
            string xmlString = doc.OuterXml;
            int len = xmlString.Length;
        }
    }

    static void Main()
    {
        XmlSerializationEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 500; i++)
        {
            SerializeToXML();
        }

        XmlSerializationEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

