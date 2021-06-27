using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Protocol.CompetitionMessages
{
    public class Login : CompetitionMessage
    {
        public Guid VmId { get; init; } = default;
    }
}
