using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Models
{
	public class AppliedCompetitionPenalty
	{
		public int TeamID { get; set; }
		public int CompetitionPenaltyID { get; set; }

		[JsonIgnore]
		public Team Team { get; set; }
		[JsonIgnore]
		public CompetitionPenalty CompetitionPenalty { get; set; }
	}
}
