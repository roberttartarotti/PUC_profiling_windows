using System;
using System.Diagnostics.Tracing;
using System.Xml;

[EventSource(Name = "RepeatedXPath-EventSource")]
public sealed class XPathEventSource : EventSource
{
    public static XPathEventSource Log = new XPathEventSource();

    [Event(1, Level = EventLevel.Informational)]
    public void ProcessingStarted() => WriteEvent(1);

    [Event(2, Level = EventLevel.Informational)]
    public void ProcessingCompleted() => WriteEvent(2);
}

class Program
{
    static void EvaluateXPath()
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(
            "<catalog>" +
            "<book id='1'><title>Book1</title><author>Author1</author><price>29.99</price></book>" +
            "<book id='2'><title>Book2</title><author>Author2</author><price>39.99</price></book>" +
            "<book id='3'><title>Book3</title><author>Author3</author><price>19.99</price></book>" +
            "<book id='4'><title>Book4</title><author>Author4</author><price>49.99</price></book>" +
            "<book id='5'><title>Book5</title><author>Author5</author><price>24.99</price></book>" +
            "</catalog>"
        );
        
        for (int i = 0; i < 5000; i++)
        {
            XmlNodeList nodes = doc.SelectNodes("//book[price > 25]");
            
            foreach (XmlNode node in nodes)
            {
                string text = node.InnerText;
                
                XmlNode titleNode = node.SelectSingleNode("title");
                if (titleNode != null)
                {
                    string title = titleNode.InnerText;
                }
            }
        }
    }

    static void Main()
    {
        XPathEventSource.Log.ProcessingStarted();

        for (int i = 0; i < 100; i++)
        {
            EvaluateXPath();
        }

        XPathEventSource.Log.ProcessingCompleted();

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}

