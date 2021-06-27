using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common.Models;

namespace Common.Protocol.CompetitionMessages
{
    public class TeamsList : CompetitionMessage
    {
        public IEnumerable<Team> Teams { get; init; }
    }
}
