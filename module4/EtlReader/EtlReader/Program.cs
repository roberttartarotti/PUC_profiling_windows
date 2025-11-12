using EtlReader;
using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class Program
{
    public static EtwManifest manifest = new EtwManifest();

    public static Dictionary<string, object> readDictionaryPayload(string xml)
    {
        string jsonPayload = readStringPayload(xml);
        if (string.IsNullOrEmpty(jsonPayload))
        {
            return new Dictionary<string, object>();
        }
        else
        {
            while(!jsonPayload.StartsWith("{"))
            {
                jsonPayload = jsonPayload.Substring(1);
            }
            return JsonSerializer.Deserialize<Dictionary<string, object>>(jsonPayload);
        }
    }

    public static string readStringPayload(string xml)
    {
        string payload = Encoding.UTF8.GetString(readBytesPayload(xml, false));
        return payload;
    }

    public static byte[] readBytesPayload(string xml, bool allBytes)
    {
        XDocument xmlDocument = XDocument.Parse(xml);
        string payload = xmlDocument.Root.XPathSelectElement("Payload").Value;

        string[] lines = payload.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        List<byte> bytesList = new List<byte>();

        foreach (string line in lines)
        {
            List<byte> byteLowList = new List<byte>();
            List<byte> byteHigtList = new List<byte>();
            for (int i = 0; i < 8; i++)
            {
                string hexLow = line.PadRight(82, ' ').Substring(11 + i * 3, 2).Trim();
                if (hexLow != string.Empty)
                {
                    byte byteLow = Convert.ToByte(hexLow.PadLeft(2, '0'), 16);
                    if ((byteLow >= 32) || allBytes)
                    {
                        byteLowList.Add(byteLow);
                    }
                }
                string hexHigt = line.PadRight(82, ' ').Substring(37 + i * 3, 2).Trim();
                if (hexHigt != string.Empty)
                {
                    byte byteHigt = Convert.ToByte(hexHigt.PadLeft(2, '0'), 16);
                    if ((byteHigt >= 32) || allBytes)
                    {
                        byteHigtList.Add(byteHigt);
                    }
                }
            }

            bytesList.AddRange(byteLowList);
            bytesList.AddRange(byteHigtList);
        }
        return bytesList.ToArray();
    }

    public static List<EventFile> events = new List<EventFile>();

    public static void ReadEtlFile(string etlFilePath)
    {
        using (var source = new ETWTraceEventSource(etlFilePath))
        {
            // Subscribe to all events
            source.AllEvents += async delegate (TraceEvent data)
            {
                if( data.ProviderGuid == new Guid("{5f506867-43a7-4eff-a2ca-a9a5212d64d4}"))
                {
                    if ((int)data.ID == 65534)
                    {
                        string stringXmlManifest = readStringPayload(data.Dump());
                        manifest.AddXml(stringXmlManifest);
                    }
                    else
                    {
                        var payload = readDictionaryPayload(data.Dump());

                        switch ((int)data.ID) {
                            case 12:
                                EventFile @event = events.Where(x => x.fileName == payload["fileName"].ToString()).FirstOrDefault();
                                if (@event == null)
                                {
                                    @event = new EventFile(payload["fileName"].ToString(), data.TimeStamp);
                                    events.Add(@event);
                                }
                                break;
                            case 13:
                                @event = events.Where(x => x.fileName == payload["fileName"].ToString()).FirstOrDefault();
                                if (@event != null)
                                {
                                    var line = @event.lines.Where(x => x.idLine == new Guid(payload["lineCode"].ToString())).FirstOrDefault();
                                    if (line == null)
                                    {
                                        line = new EventImport();
                                        line.idLine = new Guid(payload["lineCode"].ToString());
                                        line.lineText = payload["line"].ToString();
                                        line.ProcessLine = data.TimeStamp;

                                        @event.lines.Add(line);
                                    }
                                }
                                break;
                            case 14:
                                @event = events.Where(x => x.fileName == payload["fileName"].ToString()).FirstOrDefault();
                                if (@event != null)
                                {
                                    var line = @event.lines.Where(x => x.idLine == new Guid(payload["lineCode"].ToString())).FirstOrDefault();
                                    if (line != null)
                                    {
                                        line.ProcessData = data.TimeStamp;
                                        line.conta = int.Parse(payload["conta"].ToString());
                                        line.value = decimal.Parse(payload["valor"].ToString());
                                        line.descricao = payload["descricao"].ToString();
                                    }
                                }
                                break;

                            case 21:
                                @event = events.Where(x => x.fileName == payload["fileName"].ToString()).FirstOrDefault();
                                if (@event != null)
                                {
                                    @event.errorProcess = data.TimeStamp;
                                }
                                break;
                            case 22:
                                @event = events.Where(x => x.fileName == payload["fileName"].ToString()).FirstOrDefault();
                                if (@event != null)
                                {
                                    var line = @event.lines.Where(x => x.idLine == new Guid(payload["lineCode"].ToString())).FirstOrDefault();
                                    if (line != null)
                                    {
                                        line.ErrorLine = data.TimeStamp;
                                    }
                                }
                                break;
                            case 23:
                                @event = events.Where(x => x.fileName == payload["fileName"].ToString()).FirstOrDefault();
                                if (@event != null)
                                {
                                    var line = @event.lines.Where(x => x.idLine == new Guid(payload["lineCode"].ToString())).FirstOrDefault();
                                    if (line != null)
                                    {
                                        line.ErrorData = data.TimeStamp;
                                    }
                                }
                                break;
                        
                        }


                    }
                }
                // You can access more specific data based on the event type
                // For example, if data is a KernelProcessTraceData, you can access data.CommandLine
            };

            // Process the events in the ETL file
            source.Process();
        }

        foreach (var e in events)
        {
            Console.WriteLine("Inicio do processo do arquivo - " + e.fileName);
            Console.WriteLine("-" + e.startProcess.ToString("dd/MM/yyyy HH:mm:ss"));
            foreach (var line in e.lines.Where(x=>x.ErrorLine != null).ToList())
            {
                Console.WriteLine("---------------------------------------------------------------");
                Console.WriteLine("-Linha - " + line.lineText);
                Console.WriteLine("--" + line.ProcessLine.ToString("dd/MM/yyyy HH:mm:ss"));
                if (line.ErrorLine != null)
                {
                    Console.WriteLine($"--Error Linha - {line.ErrorLine.Value.ToString("dd/MM/yyyy HH:mm:ss")}");
                }
                else
                {
                    Console.WriteLine("--" + line.ProcessData.ToString("dd/MM/yyyy HH:mm:ss"));
                    if (line.ErrorData != null)
                    {
                        Console.WriteLine($"--Error Data - {line.ErrorData.Value.ToString("dd/MM/yyyy HH:mm:ss")}");
                    }
                    else
                    {
                        Console.WriteLine("--Conta :" + line.conta.ToString());
                        Console.WriteLine("--Valor :" + line.value.ToString());
                        Console.WriteLine("--Descricao :" + line.descricao);
                    }
                }
                Console.WriteLine("---------------------------------------------------------------");
            }
            if (e.errorProcess != null)
            {
                Console.WriteLine($"-Error - {e.errorProcess.Value.ToString("dd/MM/yyyy HH:mm:ss")}");
            }
        }

        Console.WriteLine("Finished processing ETL file.");
    }

    public static void Main(string[] args)
    {
        ReadEtlFile(@"C:\Projeto\Importador\Importador\ETLError.etl");
    }

}