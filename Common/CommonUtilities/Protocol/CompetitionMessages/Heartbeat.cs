using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Protocol.CompetitionMessages
{
    public class Heartbeat : CompetitionMessage
    {
        public DateTime Timestamp { get; init; } = DateTime.Now;
    }
}
