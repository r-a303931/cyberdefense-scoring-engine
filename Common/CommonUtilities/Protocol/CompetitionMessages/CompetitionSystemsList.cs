using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common.Models;

namespace Common.Protocol.CompetitionMessages
{
    public class CompetitionSystemsList : CompetitionMessage
    {
        public IEnumerable<CompetitionSystem> CompetitionSystems { get; init; }
    }
}
