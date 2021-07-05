using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EngineController.Models
{
    public class AppliedCompetitionPenalty
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TeamID { get; set; }
        public int CompetitionPenaltyID { get; set; }

        [JsonIgnore]
        public Team Team { get; set; }
        [JsonIgnore]
        public CompetitionPenalty CompetitionPenalty { get; set; }
    }
}
