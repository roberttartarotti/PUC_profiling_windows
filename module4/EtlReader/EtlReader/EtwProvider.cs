using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtlReader
{
    public class EtwProvider
    {
        public readonly Guid ProviderId;
        public readonly string Name;
        public readonly string Symbol;
        public readonly string ResourceFilename;
        public readonly string MessageFilename;
        public readonly List<EtwEvent> Events;

        public EtwProvider(Guid providerId,
            string name,
            string symbol,
            string resourceFilename,
            string messageFilename)
        {
            this.ProviderId = providerId;
            this.Name = name;
            this.Symbol = symbol;
            this.ResourceFilename = resourceFilename;
            this.MessageFilename = messageFilename;
            this.Events = new List<EtwEvent>();
        }
    }
}
