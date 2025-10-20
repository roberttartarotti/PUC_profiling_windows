using System;
using System.Diagnostics.Tracing;
using System.Xml;

[EventSource(Name = "XmlAttributeIteration-EventSource")]
public sealed class XmlAttributeEventSource : EventSource
{
    public static XmlAttributeEventSource Log = new XmlAttributeEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static void ProcessXMLAttributes()
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml("<root><item id='1' name='test' value='100' status='active' type='primary'/></root>");
        
        XmlNodeList items = doc.GetElementsByTagName("item");
        
        foreach (XmlNode item in items)
        {
            if (item.Attributes != null)
            {
                foreach (XmlAttribute attr in item.Attributes)
                {
                    string name = attr.Name;
                    string value = attr.Value;
                    
                    XmlAttribute clonedAttr = (XmlAttribute)attr.CloneNode(true);
                    string clonedValue = clonedAttr.Value;
                }
            }
        }
    }

    static void Main()
    {
        XmlAttributeEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 50000; i++)
        {
            ProcessXMLAttributes();
        }

        XmlAttributeEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

