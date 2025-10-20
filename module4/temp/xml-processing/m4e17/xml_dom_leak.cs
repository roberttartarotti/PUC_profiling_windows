using System;
using System.Diagnostics.Tracing;
using System.Xml;

[EventSource(Name = "XmlDomLeak-EventSource")]
public sealed class XmlDomEventSource : EventSource
{
    public static XmlDomEventSource Log = new XmlDomEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static void ParseXMLDocument()
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml("<root><item id='1'><data>Value1</data></item><item id='2'><data>Value2</data></item><item id='3'><data>Value3</data></item></root>");
        
        XmlNodeList nodes = doc.SelectNodes("//item");
        
        foreach (XmlNode node in nodes)
        {
            string text = node.InnerText;
            
            XmlNode clonedNode = node.CloneNode(true);
            
            XmlNodeList childNodes = node.SelectNodes(".//*");
            foreach (XmlNode child in childNodes)
            {
                string childText = child.InnerText;
            }
        }
    }

    static void Main()
    {
        XmlDomEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 5000; i++)
        {
            ParseXMLDocument();
        }

        XmlDomEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

