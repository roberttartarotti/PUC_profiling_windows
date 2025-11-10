using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtlReader
{
    public class EtwEvent
    {
        public int Value { get; set; }
        public int Version { get; set; }
        public string Level { get; set; }
        public string Symbol { get; set; }
        public string Task { get; set; }
        public string Template { get; set; }
        public KeyValuePair<string,string>[] Datas { get; set; }

        public EtwEvent(int value, int version, string level, string symbol, string task, string template)
        {
            Value = value;
            Version = version;
            Level = level;
            Symbol = symbol;
            Task = task;
            Template = template;
            Datas = new KeyValuePair<string, string>[] { };
        }

        public void AddData(string name, string type)
        {
            var dataList = Datas.ToList();
            dataList.Add(new KeyValuePair<string, string>(name, type));
            Datas = dataList.ToArray();
        }
    }
}
