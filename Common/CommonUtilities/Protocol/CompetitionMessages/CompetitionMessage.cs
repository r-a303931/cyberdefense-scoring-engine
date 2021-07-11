using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Protocol.CompetitionMessages
{
    public abstract class CompetitionMessage : IJsonProtocolMessage
    {
        /// When sending a message, this should be the new GUID. When responding,
        /// it should be overwritten with the ResponseGuid provided in the original
        /// message
        public Guid ResponseGuid { get; init; } = Guid.NewGuid();
    }
}
