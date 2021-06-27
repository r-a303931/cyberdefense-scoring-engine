using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Protocol.CompetitionMessages
{
    public class RegisterVM : CompetitionMessage
    {
        public Guid Id { get; init; }

        public int TeamId { get; init; }

        public int SystemIdentifier { get; init; }
    }
}
