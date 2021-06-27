using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Protocol.CompetitionMessages
{
    public class SetCompletedTasks : CompetitionMessage
    {
        public IEnumerable<int> TaskIds { get; init; }
    }
}
