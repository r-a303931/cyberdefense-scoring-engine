using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Protocol.CompetitionMessages
{
    public class HeartbeatAcknowledge : CompetitionMessage
    {
        public DateTime SentTimestamp { get; init; } = DateTime.Now;

        public DateTime TransferredTimestamp { get; init; }
    }
}
