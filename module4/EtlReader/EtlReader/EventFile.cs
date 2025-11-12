using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtlReader
{
    public class EventFile
    {
        public string fileName;
        public DateTime startProcess;

        public DateTime? errorProcess;

        public List<EventImport> lines = new List<EventImport>();

        public EventFile(string _fileName, DateTime date)
        {
            fileName = _fileName;
            startProcess = date;
            lines = new List<EventImport>();
        }
    }
}
