using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EngineController.Models
{
    public class AppliedCompetitionPenalty
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TeamID { get; set; }
        public int CompetitionPenaltyID { get; set; }

        public Guid VmId { get; set; }

        [JsonIgnore]
        public Team Team { get; set; }
        [JsonIgnore]
        public CompetitionPenalty CompetitionPenalty { get; set; }
        [JsonIgnore]
        public RegisteredVirtualMachine AppliedVirtualMachine { get; set; }
    }
}
