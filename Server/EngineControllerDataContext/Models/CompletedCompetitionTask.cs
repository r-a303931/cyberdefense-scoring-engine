using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EngineController.Models
{
    public class CompletedCompetitionTask
    {
        public int TeamID { get; set; }
        public int CompetitionTaskID { get; set; }

        public Guid VmId { get; set; }

        [JsonIgnore]
        public Team Team { get; set; }
        [JsonIgnore]
        public CompetitionTask CompetitionTask { get; set; }
        [JsonIgnore]
        public RegisteredVirtualMachine AppliedVirtualMachine { get; set; }
    }
}
