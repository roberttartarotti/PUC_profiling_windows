using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EtlReader
{
    public class EtwManifest
    {
        public List<EtwProvider> Providers { get; set; }

        public Dictionary<string, string> Messages { get; set; }

        public EtwManifest()
        {
            Providers = new List<EtwProvider>();
            Messages = new Dictionary<string, string>();
        }

        public void AddXml(string xml)
        {
            while (!xml.StartsWith("<"))
            {
                xml = xml.Substring(1);
            }

            XDocument xmlManifest = XDocument.Parse(xml);

            var _elements = xmlManifest.Root.Elements();

            var instrumentationNode = _elements.FirstOrDefault(e => e.Name.LocalName == "instrumentation");
            var eventsNode = instrumentationNode.Elements().FirstOrDefault(e => e.Name.LocalName == "events");
            var providers = eventsNode.Elements().Where(e => e.Name.LocalName == "provider").ToList();

            providers.ForEach(n =>
            {
                var guid = new Guid(n.Attribute("guid").Value);
                if (!Providers.Exists(x => x.ProviderId == guid))
                {
                    var name = n.Attribute("name").Value;
                    var symbol = n.Attribute("symbol").Value;
                    var resourceFilename = n.Attribute("resourceFileName").Value;
                    var messageFilename = n.Attribute("messageFileName").Value;
                    var provider = new EtwProvider(guid, name, symbol, resourceFilename, messageFilename);
                    var events = n.Elements().Where(e => e.Name.LocalName == "events").FirstOrDefault().Elements().Where(e => e.Name.LocalName == "event").ToList();
                    events.ForEach(e =>
                    {
                        var value = int.Parse(e.Attribute("value").Value);
                        var version = int.Parse(e.Attribute("version").Value);
                        var level = e.Attribute("level").Value;
                        var symbolEvent = e.Attribute("symbol").Value;
                        var task = e.Attribute("task").Value;
                        var template = e.Attribute("template")?.Value;

                        provider.Events.Add(new EtwEvent(value, version, level, symbol, task, template));
                    });

                    var templates = n.Elements().Where(e => e.Name.LocalName == "templates").FirstOrDefault().Elements().Where(e => e.Name.LocalName == "template").ToList();
                    templates.ForEach(t =>
                    {
                        var @event = provider.Events.Where(ev => ev.Template == t.Attribute("tid").Value).FirstOrDefault();
                        if(@event != null)
                        {
                            var datas = t.Elements().Where(d => d.Name.LocalName == "data").ToList();
                            datas.ForEach(d =>
                            {
                                var dataName = d.Attribute("name").Value;
                                var dataInType = d.Attribute("inType").Value;

                                @event.AddData(dataName, dataInType);
                            });
                        }
                    });
                    Providers.Add(provider);
                }
            });

            var localization = _elements.FirstOrDefault(e => e.Name.LocalName == "localization");
            var resources = localization.Elements().FirstOrDefault(e => e.Name.LocalName == "resources");
            var stringTable = resources.Elements().FirstOrDefault(e => e.Name.LocalName == "stringTable");
            var messages = stringTable.Elements().Where(e => e.Name.LocalName == "string").ToList();

            messages.ForEach(m => {
                if (!Messages.ContainsKey(m.Attribute("id").Value))
                {
                    Messages.Add(m.Attribute("id").Value, m.Attribute("value").Value);
                }
            });
        }

        public string getMessageById(Guid providerId, int eventId, Dictionary<string, object> args)
        {
            var provider = Providers.Where(x => x.ProviderId == providerId).FirstOrDefault();
            if (provider == null)
                return null;
            var @event = provider.Events.Where(e => e.Value == eventId).FirstOrDefault();
            if(@event == null)
                return null;
            if (Messages.ContainsKey($"event_{@event.Task}"))
            {
                string message = Messages[$"event_{@event.Task}"];
                MatchCollection matches = Regex.Matches(message, @"%[0-9]+");
                foreach (Match match in matches)
                {
                    int poss = int.Parse(match.Value.Substring(1)) - 1;
                    if (args.ContainsKey(@event.Datas[poss].Key))
                    {
                        message = message.Replace(match.Value, args[@event.Datas[poss].Key].ToString());
                    }
                }
                return message;
            }
            else
            {
                return null;
            }
        }
    }
}
